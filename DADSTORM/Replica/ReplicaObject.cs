using CommonClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Replica {
    public class ReplicaObject : MarshalByRefObject, ReplicaInterface {
        private static int OK =0;
        private static int SLOW = 1;
        private static int CRASHED = 1;


        private IPuppetMasterLog log;
        private Router router;  
        private bool logLevel;  // full = true, light = false
        private Operation operation;
        private Queue tupleQueue;
        private string PuppetMasterUrl;

        private string replicaAddress;
        private Dictionary<string, int> replicasState;
        private string operationName;
        private List<string> allReplicasURL;
        private Dictionary<string, int> failedPings;
        private List<string> inputs = new List<string>();

        private bool start = false;
        private int waitingTime = 0;
        private bool frozen = false;

        private bool once;
        private bool atLeastOnce;
        private Dictionary<TupleWrapper, DateTime> allTuples = new Dictionary<TupleWrapper, DateTime>();
        private HashSet<string> seenTuples = new HashSet<string>();
        private Dictionary<TupleWrapper, DecisionStructure> deciding = new Dictionary<TupleWrapper, DecisionStructure>();
        private List<TupleWrapper> processingOnMe = new List<TupleWrapper>();
        private Dictionary<string, Dictionary<string, OtherReplicaTuple>> processingOnOther =
            new Dictionary<string, Dictionary<string, OtherReplicaTuple>>();
        Thread failureDetectorThread = null;
        Thread notAssignedTuplesThread = null;

        private string _routing;
        private string _semantics;


        public bool Started {
            get { return start; }
        }

        public ReplicaObject(string PuppetMasterUrl, string routing, string semantics,
            string logLevel, Operation operation, List<string> output, string replicaAddress, string operationName,
            List<string> allAdresses, List<string> inputs) {
            tupleQueue = new Queue();
            this.PuppetMasterUrl = PuppetMasterUrl;
            this.logLevel = logLevel.Equals("full");
            this.operation = operation;
            this.replicaAddress = replicaAddress;
            this.operationName = operationName;
            allReplicasURL = allAdresses;
            failedPings = new Dictionary<string, int>();
            replicasState = new Dictionary<string, int>();

            this.inputs = inputs;
            this._routing = routing;
            this._semantics = semantics;

            foreach (string s in allReplicasURL) {
                if (!s.Equals(replicaAddress)) {
                    failedPings.Add(s, 0);
                    replicasState.Add(s, OK);
                }
            }

            string routingLower = routing.ToLower();
            char[] delimiters = { '(', ')' };
            string[] splitted = routingLower.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            once = semantics.Equals("exactly-once");
            atLeastOnce = semantics.Equals("at-least-once");

            if (once || atLeastOnce) {
                failureDetectorThread = new Thread(() => failureDetector());//TODO check if it is use in jus exacly once or some else;
                notAssignedTuplesThread = new Thread(() => notAssignedTuples());
                notAssignedTuplesThread.Start();
            }
            

            if (splitted[0].Equals("primary"))
                router = new PrimaryRouter(output, semantics);
            else if (splitted[0].Equals("random"))
                router = new RandomRouter(output, semantics);
            else
                router = new HashRouter(output, semantics, int.Parse(splitted[1]));

            // Starts the structure needed for replication
            foreach(string addr in allReplicasURL) {
                if(!addr.Equals(replicaAddress))
                    processingOnOther.Add(addr, new Dictionary<string, OtherReplicaTuple>());
            }

            // Assuming that the service is in: tcp://<PuppetMasterIP>:10001/log
            string testpuppetAddress = PuppetMasterUrl + "/log";
            try
            {
                log = (IPuppetMasterLog)Activator.GetObject(typeof(IPuppetMasterLog),
                   testpuppetAddress);
            }
            catch (Exception e)
            {

                Console.WriteLine("first e: " + e);
            }
        }

        //method used to send tuples to the owner of this buffer
        //USED BY: other replicas, input file
        public void addTuple(TupleWrapper tuple) {

            while (frozen || !start)
                Thread.Sleep(1);

            Console.WriteLine("addTuple({0})", tuple.Tuple);

            if (once || atLeastOnce) {
                int majority = (int)((allReplicasURL.Count) / 2) + 1;

                if (once) {
                    if (isOnDeciding(tuple))
                        return;

                    if (getOnProcessingOnMe(tuple) != null)
                        return;

                    if (getOnProcessingOnOther(tuple) != null)
                        return;
                }
                if (isOnSeenTuples(tuple))
                    return;

                // ALL TUPLES
                Monitor.Enter(allTuples);
                if (!allTuples.ContainsKey(tuple))
                    allTuples.Add(tuple, new DateTime());
                else
                    allTuples[tuple] = new DateTime();
                Monitor.Pulse(allTuples);
                Monitor.Exit(allTuples);

                int counter = 1; // To be used for calculating the minimum required number of working replicas
                object o = new object();
                foreach (string otherReplica in allReplicasURL) {
                    if (!otherReplica.Equals(replicaAddress)) {
                        Thread t = new Thread(() => broadcastTuple(tuple, otherReplica, ref counter, ref o));
                        t.Start();
                    }
                }

                while (true) {
                    lock(o) {
                        if (counter >= majority)
                            break;
                    }
                    Thread.Sleep(1);
                }

                if (once) {
                    Thread thread = new Thread(() => chooseProcessingReplica(tuple));
                    thread.Start();
                    return;
                }
            }

            addToQueue(tuple, tupleQueue);
        }

        //method used to get tuples from the buffer
        //USED BY: owner(replica)
        public TupleWrapper getTuple() {
            Console.WriteLine("getTuple()");
            TupleWrapper t = takeFromQueue(tupleQueue);
            Console.WriteLine("         GOT tuple");
            return t;
        }

        //Command to start working
        //USED BY:PuppetMaster
        public void Start() {
            Console.WriteLine("-->START command received");
            start = true;
        }

        //Command  to set the time to wait between tuple processing
        //USED BY:PuppetMaster
        public void Interval(int time) {
            while (frozen)
                Thread.Sleep(1);

            Console.WriteLine("-->Interval command received time={0}",time);
            waitingTime = time;
        }

        private void testLog() {
            try {
                Console.WriteLine("Replica Address: " + replicaAddress + "\r\nOperation: " +
                    operationName + "\r\nFLAGS\r\nStart: " + start + "\r\nWaiting: " +
                    waitingTime + "\r\nFrozen: " + frozen);
            } catch (Exception ex) {
                Console.WriteLine("second ex: " + ex);
            }
        }

        //Command to print the current status
        //USED BY:PuppetMaster
        public void Status() {
            while (frozen)
                Thread.Sleep(1);

            Console.WriteLine("-->STATUS command received");

            new Thread(() => testLog()).Start();
        }

        //Command to simulate a program crash
        //USED BY:PuppetMaster 
        public void Crash() {
            Console.WriteLine("-->CRASH command received");
            Process.GetCurrentProcess().Kill();
            //consumer.Abort();
        }

        //Command used to simulate slow server
        //USED BY:PuppetMaster
        public void Freeze() {
            Console.WriteLine("-->FREEZE command received");
            frozen = true;
        }

        //Command used to end the slow server simulation
        //USED BY:PuppetMaster
        public void Unfreeze() {
            Console.WriteLine("-->UNFREEZE command received");
            frozen = false;
        }


        //USED BY: other replica
        public bool wasElementSeen(string s) {
            return operation.wasElementSeen(s);
        }

        //USED BY: other replica
        public int numberOfProcessedTuples() {
            return operation.numberOfProcessedTuples();
        }

        // To be used in the consumer thread
        public void Operate() {
            Console.WriteLine("6-Waiting for START command");
            int counter = 0;

           

            while (!start)
                Thread.Sleep(100);

            Console.WriteLine("4-If needed creating File reader");

            foreach (string input in inputs)
                if (input.EndsWith(".dat") || input.EndsWith(".data"))
                {

                    Process process = new Process();
                    process.StartInfo.FileName = "..\\..\\..\\ReadTuplesFromFile\\bin\\Debug\\ReadTuplesFromFile.exe";
                    process.StartInfo.Arguments = input + " " + _routing + " " + _semantics;
                    foreach (string s in allReplicasURL)
                        process.StartInfo.Arguments += " " + s;

                    //var thread_process = new Thread(() =>
                    //{
                    //    process.Start();
                    //});

                    //thread_process.Start();
                    //thread_process.Join();
                    process.Start();
                }



            if (failureDetectorThread != null) 
                failureDetectorThread.Start();//TODO check if this is used just in exacly once
            
            while (true) {
                int majority = (int)((allReplicasURL.Count) / 2) + 1;

                //see if it is feezed
                while (frozen == true)
                    Thread.Sleep(100);

                //wait the defined time between processing
                Thread.Sleep(waitingTime);

                //get tuple from the buffer
                TupleWrapper tuple = getTuple();

                List<string[]> result = operation.Operate(tuple.Tuple);

                Console.WriteLine("after operation.Operate result params:");

                if (result != null) {
                    foreach (var a in result)
                        Console.WriteLine("  " + a);

                    List<TupleWrapper> convertedResult = new List<TupleWrapper>();

                    foreach (string[] el in result)
                        convertedResult.Add(new TupleWrapper(tuple.ID, "" + counter++ + ":" + replicaAddress, el));

                    if (once) {
                        int n = 1; // To be used for calculating the minimum required number of working replicas
                        object o = new object();
                        foreach (string otherReplica in allReplicasURL) {
                            if (!otherReplica.Equals(replicaAddress)) {
                                Thread t = new Thread(() => broadcastResult(tuple, otherReplica, convertedResult, ref n, ref o));
                                t.Start();
                            }
                        }

                        while (true) {
                            lock (o) {
                                if (n >= majority)
                                    break;
                            }
                            Thread.Sleep(1);
                        }
                    }

                    foreach (TupleWrapper outTuple in convertedResult){
                        Console.WriteLine("sending tuple");
                        router.sendToNext(outTuple);

                        if (logLevel)
                            log.Log("tuple " + operationName + " " + replicaAddress + " <" + string.Join(" - ", outTuple.Tuple) + ">" );
                    }

                    if (once) {
                        Monitor.Enter(seenTuples);
                        seenTuples.Add(tuple.ID);
                        Monitor.Pulse(seenTuples);
                        Monitor.Exit(seenTuples);

                        Monitor.Enter(processingOnMe);
                        processingOnMe.Remove(tuple);
                        Monitor.Pulse(processingOnMe);
                        Monitor.Exit(processingOnMe);

                        int n = 1; // To be used for calculating the minimum required number of working replicas
                        object o = new object();
                        foreach (string otherReplica in allReplicasURL) {
                            if (!otherReplica.Equals(replicaAddress)) {
                                Thread t = new Thread(() => broadcastFinished(tuple, otherReplica, ref n, ref o));
                                t.Start();
                            }
                        }

                        while (true) {
                            lock (o) {
                                if (n >= majority)
                                    break;
                            }
                            Thread.Sleep(1);
                        }
                    }

                    // To remove the tuples from the other lists
                    else if (once) {
                        Monitor.Enter(seenTuples);
                        seenTuples.Add(tuple.ID);
                        Monitor.Pulse(seenTuples);
                        Monitor.Exit(seenTuples);

                        int n = 1; // To be used for calculating the minimum required number of working replicas
                        object o = new object();
                        foreach (string otherReplica in allReplicasURL) {
                            if (!otherReplica.Equals(replicaAddress)) {
                                Thread t = new Thread(() => broadcastFinished(tuple, otherReplica, ref n, ref o));
                                t.Start();
                            }
                        }

                        while (true) {
                            lock (o) {
                                if (n >= majority)
                                    break;
                            }
                            Thread.Sleep(1);
                        }
                    }
                }
                else
                    Console.WriteLine(" result of operation.Operate was null ???? ");
            }
        }

        /***************************
         * FAULT-TOLERANCE METHODS *
         **************************/

        // To be used locally
        private void broadcastTuple(TupleWrapper t, string url, ref int counter, ref object o) {
            ReplicaInterface r;
            if ((r = getReplica(url)) != null) {
                try {
                    r.arrivedTuple(t);
                    lock (o) {
                        counter++;
                    }
                } catch(System.Net.Sockets.SocketException) { }
            }
        }

        // To be used locally
        private void broadcastResult(TupleWrapper t, string url, List<TupleWrapper> result, ref int counter, ref object o) {
            ReplicaInterface r;
            if ((r = getReplica(url)) != null) {
                try {
                    r.finishedProcessing(t.ID, result, replicaAddress);

                    lock (o) {
                        counter++;
                    }
                }
                catch (System.Net.Sockets.SocketException) { }
            }
        }

        // To be used locally
        private void broadcastFinished(TupleWrapper t, string url, ref int counter, ref object o) {
            ReplicaInterface r;
            if ((r = getReplica(url)) != null) {
                try {
                    r.finishedSending(t, replicaAddress);

                    lock (o) {
                        counter++;
                    }
                }
                catch (System.Net.Sockets.SocketException) { }
            }
        }

        public void arrivedTuple(TupleWrapper t) {
            while (frozen)
                Thread.Sleep(1);

            if (once) {
                if (isOnDeciding(t))
                    return;

                if (getOnProcessingOnMe(t)!=null)
                    return;


                if (getOnProcessingOnOther(t)!=null)
                    return;
            }

            if (isOnSeenTuples(t))
                return;

            Monitor.Enter(allTuples);
            if (!allTuples.ContainsKey(t))
                allTuples.Add(t, new DateTime());
            else
                allTuples[t] = new DateTime();
            Monitor.Pulse(allTuples);
            Monitor.Exit(allTuples);
        }

        // To check the threads timestamps
        private void notAssignedTuples() {
            int TIME_INTERVAL = 20000; // measured in milliseconds

            while(true) {
                Dictionary<TupleWrapper, DateTime> toAnalyze;
                Monitor.Enter(allTuples);
                toAnalyze = new Dictionary<TupleWrapper, DateTime>(allTuples);
                Monitor.Pulse(allTuples);
                Monitor.Exit(allTuples);

                foreach (KeyValuePair<TupleWrapper, DateTime> entry in toAnalyze) {
                    DateTime now = new DateTime();
                    TimeSpan diff = now - entry.Value;
                    bool isInsideInterval = (int) Math.Abs(diff.TotalMilliseconds) < TIME_INTERVAL;

                    if (atLeastOnce && !isInsideInterval) {
                        Monitor.Enter(allTuples);
                        allTuples.Remove(entry.Key);
                        Monitor.Pulse(allTuples);
                        Monitor.Exit(allTuples);

                        addToQueue(entry.Key, tupleQueue);
                    }

                    else if(once && !isInsideInterval) {
                        // TODO: Implement consensus!!!
                    }
                }

                Thread.Sleep(1);
            }
        }

        public void tryElectionOfProcessingReplica(TupleWrapper t, string url) {
            while (frozen)
                Thread.Sleep(1);

            Monitor.Enter(deciding);
            if (deciding.ContainsKey(t)) {
                Monitor.Pulse(deciding);
                Monitor.Exit(deciding);
                throw new AlreadyVotedException();
            }

            deciding.Add(t, new DecisionStructure(url));
            Monitor.Pulse(deciding);
            Monitor.Exit(deciding);
        }

        public void confirmElection(TupleWrapper t, string url) {
            while (frozen)
                Thread.Sleep(1);

            // Remove from deciding pile
            Monitor.Enter(deciding);
            if(deciding.ContainsKey(t))
                deciding.Remove(t);
            Monitor.Pulse(deciding);
            Monitor.Exit(deciding);

            // Remove from allTuples
            Monitor.Enter(allTuples);
            allTuples.Remove(t);
            Monitor.Pulse(allTuples);
            Monitor.Exit(allTuples);

            // Add to the respective processing pile
            if (url.Equals(replicaAddress)) {
                addToQueue(t, tupleQueue);
                Monitor.Enter(processingOnMe);
                processingOnMe.Add(t);
                Monitor.Pulse(processingOnMe);
                Monitor.Exit(processingOnMe);
            }

            else {
                Monitor.Enter(processingOnOther);
                if (!processingOnOther.ContainsKey(url))
                    processingOnOther.Add(url, new Dictionary<string, OtherReplicaTuple>());
                processingOnOther[url].Add(t.ID, new OtherReplicaTuple(t));
                Monitor.Pulse(processingOnOther);
                Monitor.Exit(processingOnOther);
            }
        }

        public void finishedProcessing(string tupleID, List<TupleWrapper> result, string url) {
            while (frozen)
                Thread.Sleep(1);

            Monitor.Enter(processingOnOther);
            Dictionary<string, OtherReplicaTuple> t = processingOnOther[url];
            Monitor.Pulse(processingOnOther);
            Monitor.Exit(processingOnOther);
            t[tupleID].finishedProcessing(result);
        }

        public void finishedSending(TupleWrapper tuple, string url) {
            while (frozen)
                Thread.Sleep(1);

            Monitor.Enter(seenTuples);
            seenTuples.Add(tuple.ID);
            Monitor.Pulse(seenTuples);
            Monitor.Exit(seenTuples);

            if (once) {
                Monitor.Enter(processingOnOther);
                Dictionary<string, OtherReplicaTuple> t = processingOnOther[url];
                t.Remove(tuple.ID);
                Monitor.Pulse(processingOnOther);
                Monitor.Exit(processingOnOther);
            }

            else if(atLeastOnce) {
                Monitor.Enter(allTuples);
                allTuples.Remove(tuple);
                Monitor.Pulse(processingOnOther);
                Monitor.Exit(processingOnOther);
            }
        }

        /// <summary>Used to see if the replica is ok</summary>
        public bool ping(string originUrl,bool sameLayer) {
            while (frozen) { Thread.Sleep(100); }

            //Console.WriteLine("Ping requested by{0} in {1}", originUrl, replicaAddress);
            return true;
        }

        /// <summary>Infinit cicle  for checking the state of other replicas ant take mesures</summary>
        public void failureDetector() {
            while (true) {
                Thread.Sleep(5000);
                List<string> otherReplicasURLcopy = allReplicasURL.ToList();
                otherReplicasURLcopy.Remove(replicaAddress);
                foreach (string url in otherReplicasURLcopy) {
                    Console.WriteLine("ping to {0}", url);
                    try {
                        Exception exception = null;
                        ReplicaInterface replica = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), url);
                        var thread = new Thread(() => {
                            try {
                                replica.ping(replicaAddress, true);
                            } catch (Exception e) { exception = e; }
                        });
                        thread.Start();
                        var completed = thread.Join(500);
                        if (!completed) {
                            completed = thread.Join(500);
                            if (!completed) {
                                completed = thread.Join(1000);
                                if (!completed) {
                                    completed = thread.Join(1000);
                                    if (!completed) {
                                        completed = thread.Join(2000);
                                    }
                                }
                            }
                        }
                        if (!completed) {
                            Console.WriteLine("!!!ping to {0} failed", url);
                            failedPings[url] = failedPings[url] + 1;
                            thread.Abort();
                            if (failedPings[url] == 6) {//TODO reduce the time needed to fail, or not
                                failedPings[url] = 0;
                                handleSlowReplica(url);
                            }
                        }

                        if (exception != null) {
                            Console.WriteLine("!!! {0} is crashed", url);
                            handleCrashedReplica(url);
                        }
                    } catch { Console.WriteLine("!!!exception on failure detection"); }
                }
            }
        }

        /// <summary>Handles the crash of the replica specified in the arg:url</summary>
        private void handleCrashedReplica(string url) {
            Console.WriteLine("!!!handleCrashedReplica({0})", url);
            lock (allReplicasURL) {  allReplicasURL.Remove(url); }//TODO CHECK if we can do this !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            string responsable = consensusForHandleCrash(url);
            if (responsable.Equals(replicaAddress)) 
                changeToMe(url);
            else
                changeOwner(url, responsable); 
        }

        /// <summary>Handles the Slow of the replica specified in the arg:url</summary>
        private void handleSlowReplica(string url) {
            Console.WriteLine("!!!handleSlowReplica({0})", url);
                }//TODO


        /// <summary>Decide wich replica will be resplonsable to do the work of the crashed replica specified in the arg:url</summary>
        private string consensusForHandleCrash(string url) {
            //TODO make consensus to chosse who is going to handle the crashed
            //string consensusWiner = replicaAddress;

            //if winer merge other tuples with mines
            string consensusWiner = "xxx";


            return consensusWiner;
        }

        /// <summary>Change the owner of the tuples from oldOwner to newOwner</summary>
        private Dictionary<string, OtherReplicaTuple> changeOwner(string oldOwner,string newOwner) {
            Dictionary<string, OtherReplicaTuple> oldtuples = new Dictionary<string, OtherReplicaTuple>(processingOnOther[oldOwner]);
            foreach (string tupleid in oldtuples.Keys) {
                processingOnOther[newOwner].Add(tupleid, processingOnOther[oldOwner][tupleid]);
                processingOnOther[oldOwner].Remove(tupleid);
            }
            return oldtuples;
        }

        /// <summary>Change the owner of the tuples from oldOwner to this replica</summary>
        private void changeToMe(string oldOwner) {//TODO make locks

            Dictionary<string, OtherReplicaTuple> notProcessed = new Dictionary<string, OtherReplicaTuple>();
            Dictionary <string, OtherReplicaTuple> oldtuples =new Dictionary<string, OtherReplicaTuple>( processingOnOther[oldOwner]);

            foreach (string tupleid in oldtuples.Keys) {
                if (oldtuples[tupleid].isProcessed()) {//Needs a majority
                    Monitor.Enter(processingOnOther);
                    TupleWrapper tuple = processingOnOther[oldOwner][tupleid].getTuple();
                    List<TupleWrapper> result = processingOnOther[oldOwner][tupleid].getResult();
                    processingOnOther[oldOwner].Remove(tupleid);
                    Monitor.Pulse(processingOnOther);
                    Monitor.Exit(processingOnOther);

                    Monitor.Enter(seenTuples);
                    seenTuples.Add(tupleid);
                    Monitor.Pulse(seenTuples);
                    Monitor.Exit(seenTuples);

                    foreach (TupleWrapper tup in result) {
                        int n = 1; // To be used for calculating the minimum required number of working replicas
                        object o = new object();//TODO need timeout
                        foreach (string otherReplica in allReplicasURL) {
                            if (!otherReplica.Equals(replicaAddress)) {
                                Thread thread = new Thread(() => broadcastFinished(tup, otherReplica, ref n, ref o));
                                thread.Start();
                            }
                        }

                        int x = ((allReplicasURL.Count) / 2) + 1;
                        while (true) {
                            lock (o) {
                                if (n == x)
                                    break;
                            }
                            Thread.Sleep(1);
                        }
                    }
                }
                else {//needs all the others information
                    notProcessed.Add(tupleid, oldtuples[tupleid]);
                    Monitor.Enter(processingOnOther);
                    processingOnOther[oldOwner].Remove(tupleid);
                    Monitor.Pulse(processingOnOther);
                    Monitor.Exit(processingOnOther);
                }
            }
            Thread t = new Thread(() => handleNotProcessed(notProcessed, allReplicasURL,replicaAddress));
            t.Start();
        }

        private static void handleNotProcessed(Dictionary<string, OtherReplicaTuple> notProcessed,List<string> allReplicasURL, string myUrl) {
            if (notProcessed.Count == 0)
                return;

            ReplicaInterface r;
            foreach (string tupleid in notProcessed.Keys) {
                foreach (string url in allReplicasURL) {
                    if (!url.Equals(myUrl)) {
                        try {
                            r = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), url);
                            //r.getstate(id);

                        } catch (System.Net.Sockets.SocketException) { }
                    } 


                }

            }


       }

        

        /********************
         * AGREEMENT THREAD *
         *******************/
        private void chooseProcessingReplica(TupleWrapper t) {
            int majority = (int)((allReplicasURL.Count) / 2) + 1;

            int counter = 0;
            foreach (string url in allReplicasURL) {
                ReplicaInterface r;

                if ((r = getReplica(url)) == null)
                    continue;

                int exception = -1;
                Thread thread = new Thread(() =>
                {
                    try {
                        tryElectionOfProcessingReplicaRelay(t, r, replicaAddress);
                    }
                    catch (AlreadyVotedException) {
                        exception = 0;
                    }
                    catch(Exception) { }
                });

                thread.Start();
                if (!thread.Join(3000)) {
                    thread.Abort();
                    if (exception == 0)
                        return;
                }

                else
                    counter++;

                if (counter == majority)
                    break;
            }

            counter = 0;
            object o = new object();
            foreach(string replica in allReplicasURL) {
                ReplicaInterface r;

                if ((r = getReplica(replica)) == null)
                    continue;

                Thread thread = new Thread(() => {
                    confirmElectionRelay(t, r, replicaAddress, ref counter, ref o);
                });
                thread.Start();
            }

            while (true) {
                lock (o) {
                    if (counter >= majority)
                        break;
                }
                Thread.Sleep(1);
            }
        }

        private void tryElectionOfProcessingReplicaRelay(TupleWrapper t, ReplicaInterface r, string replicaAddress) {
            r.tryElectionOfProcessingReplica(t, replicaAddress);
        }

        private void confirmElectionRelay(TupleWrapper t, ReplicaInterface r, string replicaAddr, ref int counter, ref object o) {
            r.confirmElection(t, replicaAddress);

            lock(o) {
                counter++;
            }
        }

        public DTOtupleState getTupleState(TupleWrapper tuple) {
            if (isOnSeenTuples(tuple)) {
                return new DTOtupleState(tuple.ID);
            }

            if (getOnProcessingOnMe(tuple) != null) {
                return new DTOtupleState(tuple, replicaAddress);
            }

            DTOtupleState result =  getOnProcessingOnOther(tuple);
            if  (result != null)
                return result;

            if (isOnDeciding(tuple) || isOnAllTuples(tuple))
                return new DTOtupleState(tuple);

            else
                return new DTOtupleState();
        }

        /*****************
         * AUX FUNCTIONS *
         ****************/
        public void addToQueue(TupleWrapper t, Queue q) {
            Monitor.Enter(q.SyncRoot);
            q.Enqueue(t);
            Monitor.PulseAll(q.SyncRoot);
            Monitor.Exit(q.SyncRoot);
        }

        public TupleWrapper takeFromQueue(Queue q) {
            Monitor.Enter(q.SyncRoot);
            while (q.Count == 0)
                Monitor.Wait(q.SyncRoot);
            TupleWrapper result = (TupleWrapper)q.Dequeue();
            Monitor.Pulse(q.SyncRoot);
            Monitor.Exit(q.SyncRoot);
            return result;
        }

        private ReplicaInterface getReplica(string url) {
            try {
                ReplicaInterface ri = (ReplicaInterface) Activator.GetObject(typeof(ReplicaInterface), url);
                return ri;
            }
            catch (System.Net.Sockets.SocketException e) {
                Console.WriteLine("Error with host " + url);
                Console.WriteLine(e);
            }

            return null;
        }

        public bool isOnDeciding(TupleWrapper t) {
            Monitor.Enter(deciding);
            if (deciding.ContainsKey(t)) {
                Monitor.Pulse(deciding);
                Monitor.Exit(deciding);
                return true;
            }
            Monitor.Pulse(deciding);
            Monitor.Exit(deciding);
            return false;
        }

        public TupleWrapper getOnProcessingOnMe(TupleWrapper t) {
            Monitor.Enter(processingOnMe);
            if (processingOnMe.Contains(t)) {
                Monitor.Pulse(processingOnMe);
                Monitor.Exit(processingOnMe);
                return t;
            }
            Monitor.Pulse(processingOnMe);
            Monitor.Exit(processingOnMe);
            return null;
        }

        public DTOtupleState getOnProcessingOnOther(TupleWrapper t) {
            DTOtupleState output;
            Monitor.Enter(processingOnOther);
            foreach (KeyValuePair<string, Dictionary<string, OtherReplicaTuple>> entry in processingOnOther) {
                foreach (Dictionary<string, OtherReplicaTuple> entry2 in processingOnOther.Values) {
                    foreach (KeyValuePair<string, OtherReplicaTuple> entry3 in entry2) {
                        if (t.Equals(entry3.Value.getTuple())) {                         
                            output = new DTOtupleState(entry3.Value, entry.Key);
                            Monitor.Pulse(processingOnOther);
                            Monitor.Exit(processingOnOther);
                            return output;
                        }
                    }
                }
            }
            Monitor.Pulse(processingOnOther);
            Monitor.Exit(processingOnOther);
            return null;
        }

        public bool isOnSeenTuples(TupleWrapper t) {
            Monitor.Enter(seenTuples);
            if (seenTuples.Contains(t.ID)) {
                Monitor.Pulse(seenTuples);
                Monitor.Exit(seenTuples);
                return true;
            }
            Monitor.Pulse(seenTuples);
            Monitor.Exit(seenTuples);
            return false;
        }

        public bool isOnAllTuples(TupleWrapper t) {
            Monitor.Enter(allTuples);
            if (allTuples.ContainsKey(t)) {
                Monitor.Pulse(allTuples);
                Monitor.Exit(allTuples);
                return true;
            }
            Monitor.Pulse(allTuples);
            Monitor.Exit(allTuples);
            return false;
        }        
    }
}
