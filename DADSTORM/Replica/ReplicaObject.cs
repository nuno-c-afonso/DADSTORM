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
        private IPuppetMasterLog log;
        private Router router;  
        private bool logLevel;  // full = true, light = false
        private Operation operation;
        private Queue tupleQueue;
        private string PuppetMasterUrl;

        private string replicaAddress;
        private string operationName;
        private List<string> otherReplicasURL;
        private Dictionary<string, short> failedPings;


        private bool start = false;
        private int waitingTime = 0;
        private bool frozen = false;

        private bool once;
        private HashSet<string> seenTuples = new HashSet<string>();
        private Dictionary<string, Dictionary<string, OtherReplicaTuple>> processingOnOther =
            new Dictionary<string, Dictionary<string, OtherReplicaTuple>>();
        private Dictionary<TupleWrapper, DecisionStructure> deciding = new Dictionary<TupleWrapper, DecisionStructure>();

        private Dictionary<TupleWrapper, DateTime> allTuples = new Dictionary<TupleWrapper, DateTime>();

        public bool Started {
            get { return start; }
        }

        public ReplicaObject(string PuppetMasterUrl, string routing, string semantics,
            string logLevel, Operation operation, List<string> output, string replicaAddress, string operationName,
            List<string> otherAdresses) {
            tupleQueue = new Queue();

            this.PuppetMasterUrl = PuppetMasterUrl;
            this.logLevel = logLevel.Equals("full");
            this.operation = operation;
            this.replicaAddress = replicaAddress;
            this.operationName = operationName;
            otherReplicasURL = otherAdresses;

            string routingLower = routing.ToLower();
            char[] delimiters = { '(', ')' };
            string[] splitted = routingLower.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            once = semantics.Equals("exactly-once");

            if (splitted[0].Equals("primary"))
                router = new PrimaryRouter(output, semantics);
            else if (splitted[0].Equals("random"))
                router = new RandomRouter(output, semantics);
            else
                router = new HashRouter(output, semantics, int.Parse(splitted[1]));

            // Starts the structure needed for replication
            foreach(string addr in otherAdresses) {
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
            Console.WriteLine("addTuple({0})", tuple.Tuple);

            // TODO: Check with other replicas
            // TODO: Check on the other structures
            // TODO: Must start as the same stuff as when it's new
            // TODO: Return a string, saying if it was decided or if it is final
            // TODO: See the behavior when it was decided, but the replica crashed
            if (once) {
                Monitor.Enter(allTuples);
                if (!allTuples.ContainsKey(tuple))
                    allTuples.Add(tuple, new DateTime());
                else
                    allTuples[tuple] = new DateTime();
                Monitor.Pulse(allTuples);
                Monitor.Exit(allTuples);

                int counter = 1; // To be used for calculating the minimum required number of working replicas
                object o = new object();
                foreach (string otherReplica in otherReplicasURL) {
                    Thread t = new Thread(() => broadcastTuple(tuple, otherReplica, ref counter, ref o));
                }

                int x = ((otherReplicasURL.Count + 1) / 2) + 1;
                while (true) {
                    lock(o) {
                        if (counter == x)
                            break;
                    }
                }

                Thread thread = new Thread(() => chooseProcessingReplica(tuple));
                thread.Start();
                return;
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

            while (true) {
                //see if it is feezed
                while(frozen == true)
                    Thread.Sleep(100);

                //wait the defined time between processing
                Thread.Sleep(waitingTime);

                //get tuple from the buffer
                TupleWrapper tuple = getTuple();

                List<string[]> result = operation.Operate(tuple.Tuple);
                if (result != null) {
                    foreach (string[] outTuple in result){
                        Console.WriteLine("sending tuple");
                        TupleWrapper t = new TupleWrapper(tuple.ID, "" + counter++ + ":" + replicaAddress, outTuple);
                        router.sendToNext(t);

                        if (logLevel)
                            log.Log("tuple " + operationName + " " + replicaAddress + " <" + string.Join(" - ", outTuple) + ">" );
                    }
                }
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
        
        public void arrivedTuple(TupleWrapper t) {
            Monitor.Enter(allTuples);
            if (!allTuples.ContainsKey(t))
                allTuples.Add(t, new DateTime());
            else
                allTuples[t] = new DateTime();
            Monitor.Pulse(allTuples);
            Monitor.Exit(allTuples);
        }

        public DecisionStructure confirmProcessingReplica(TupleWrapper t, string url) {
            DecisionStructure ds = deciding[t];

            if(ds.IsFinal)
                return ds;

            ds.IsFinal = true;
            ds.URL = url;
            return null;
        }

        public void finishedProcessing(string tupleID, List<TupleWrapper> result, string url) {
            Dictionary<string, OtherReplicaTuple> t = processingOnOther[url];
            t[tupleID].finishedProcessing(result);
        }

        public void finishedSending(string tupleID, string url) {
            seenTuples.Add(tupleID);

            Dictionary<string, OtherReplicaTuple> t = processingOnOther[url];
            t.Remove(tupleID);
        }

        public bool ping(string originUrl,bool sameLayer) {
            if (sameLayer) {
                if (!otherReplicasURL.Contains(originUrl)) 
                    otherReplicasURL.Add(originUrl);
            }
            return true;
        }

        public void failureDetector() {
           /* Thread t = new Thread(() => replica.addTuple(tuple));
            t.Start();
            if (!t.Join(TimeSpan.FromSeconds(TIMEOUT_VALUE))) {
                t.Abort();
                throw new CouldNotSendTupleException();
            }*/
        }


        /********************
         * AGREEMENT THREAD *
         *******************/

        /*
                Dictionary<string, string> alsoReceivedUrls = new Dictionary<string, string>();
                alsoReceivedUrls.Add(replicaAddress, replicaAddress);

                // TODO: Copy the tuple to the majority of replicas, in order to have shared memory
                foreach (string otherReplica in otherReplicasURL) {
                    ReplicaInterface r;

                    // TODO: Confirm if this will be a problem!!!
                    if ((r = getReplica(otherReplica)) == null)
                        continue;

                    // Receives all the information from other replicas
                    alsoReceivedUrls.Add(otherReplica, r.arrivedTuple(tuple, replicaAddress));
                }
        */

        private void chooseProcessingReplica(TupleWrapper t) {
            /*
            Dictionary<string, int> countingResult = new Dictionary<string, int>();
            foreach (KeyValuePair<string, string> entry in d) {
                if (countingResult.ContainsKey(entry.Value))
                    countingResult[entry.Value]++;
                else
                    countingResult[entry.Value] = 1;
            }

            string chosen = null;
            int value = 0;
            foreach (KeyValuePair<string, int> entry in countingResult) {
                if(chosen == null) {
                    chosen = entry.Key;
                    value = entry.Value;
                    continue;
                }

                if(entry.Value > value || (entry.Value == value && String.Compare(chosen, entry.Key) > 0)) {
                    chosen = entry.Key;
                    value = entry.Value;
                }
            }

            // TODO: Share the result with all replicas


            // TODO: Move the tuple to the other correct structure
            */
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
    }
}
