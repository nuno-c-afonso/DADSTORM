﻿using CommonClasses;
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
        private Dictionary<string, int> failedPings;


        private bool start = false;
        private int waitingTime = 0;
        private bool frozen = false;

        private bool once;
        private Dictionary<TupleWrapper, DateTime> allTuples = new Dictionary<TupleWrapper, DateTime>();
        private HashSet<string> seenTuples = new HashSet<string>();
        private Dictionary<TupleWrapper, DecisionStructure> deciding = new Dictionary<TupleWrapper, DecisionStructure>();
        private List<TupleWrapper> processingOnMe = new List<TupleWrapper>();
        private Dictionary<string, Dictionary<string, OtherReplicaTuple>> processingOnOther =
            new Dictionary<string, Dictionary<string, OtherReplicaTuple>>();
        Thread failureDetectorThread = null;


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
            failedPings = new Dictionary<string, int>();
            foreach (string s in otherReplicasURL)
                failedPings.Add(s, 0);

            string routingLower = routing.ToLower();
            char[] delimiters = { '(', ')' };
            string[] splitted = routingLower.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            once = semantics.Equals("exactly-once");
            failureDetectorThread = new Thread(() => failureDetector());//TODO check if it is use in jus exacly once or some else;
            

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

            while (frozen || !start)
                Thread.Sleep(1);

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
                    t.Start();
                }

                int x = ((otherReplicasURL.Count + 1) / 2) + 1;
                while (true) {
                    lock(o) {
                        if (counter == x)
                            break;
                    }
                    Thread.Sleep(1);
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

            if(failureDetectorThread != null) 
                failureDetectorThread.Start();//TODO check if this is used just in exacly once
            
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
                    List<TupleWrapper> convertedResult = new List<TupleWrapper>();

                    foreach (string[] el in result)
                        convertedResult.Add(new TupleWrapper(tuple.ID, "" + counter++ + ":" + replicaAddress, el));

                    if (once) {
                        int n = 1; // To be used for calculating the minimum required number of working replicas
                        object o = new object();
                        foreach (string otherReplica in otherReplicasURL) {
                            Thread t = new Thread(() => broadcastResult(tuple, otherReplica, convertedResult, ref n, ref o));
                            t.Start();
                        }

                        int x = ((otherReplicasURL.Count + 1) / 2) + 1;
                        while (true) {
                            lock (o) {
                                if (n == x)
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
                        int n = 1; // To be used for calculating the minimum required number of working replicas
                        object o = new object();
                        foreach (string otherReplica in otherReplicasURL) {
                            Thread t = new Thread(() => broadcastFinished(tuple, otherReplica, ref n, ref o));
                            t.Start();
                        }

                        int x = ((otherReplicasURL.Count + 1) / 2) + 1;
                        while (true) {
                            lock (o) {
                                if (n == x)
                                    break;
                            }
                            Thread.Sleep(1);
                        }
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
                    r.finishedSending(t.ID, replicaAddress);

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

            Monitor.Enter(allTuples);
            if (!allTuples.ContainsKey(t))
                allTuples.Add(t, new DateTime());
            else
                allTuples[t] = new DateTime();
            Monitor.Pulse(allTuples);
            Monitor.Exit(allTuples);
        }

        public bool tryElectionOfProcessingReplica(TupleWrapper t, string url) {
            while (frozen)
                Thread.Sleep(1);

            Monitor.Enter(deciding);
            if (deciding.ContainsKey(t)) {
                Monitor.Pulse(deciding);
                Monitor.Exit(deciding);
                return false;
            }

            deciding.Add(t, new DecisionStructure(url));
            Monitor.Pulse(deciding);
            Monitor.Exit(deciding);
            return true;
        }

        public void confirmElection(TupleWrapper t) {
            while (frozen)
                Thread.Sleep(1);

            // Remove from deciding pile
            Monitor.Enter(deciding);
            string replica = deciding[t].URL;
            deciding.Remove(t);
            Monitor.Pulse(deciding);
            Monitor.Exit(deciding);

            // Remove from allTuples
            Monitor.Enter(allTuples);
            allTuples.Remove(t);
            Monitor.Pulse(allTuples);
            Monitor.Exit(allTuples);

            // Add to the respective processing pile
            if (replica.Equals(replicaAddress)) {
                addToQueue(t, tupleQueue);
                Monitor.Enter(processingOnMe);
                processingOnMe.Add(t);
                Monitor.Pulse(processingOnMe);
                Monitor.Exit(processingOnMe);
            }

            else {
/* TODO: Remove
Console.WriteLine("\t\t\t\tOTHER REPLICA URL: " + replica);
Console.WriteLine("\t\t\t\tTUPLE CONTENT: <" + string.Join(" - ", t.Tuple) + ">");
Console.WriteLine("\t\t\t\tTUPLE ID: " + t.ID);
*/
                Monitor.Enter(processingOnOther);
                if (!processingOnOther.ContainsKey(replica))
                    processingOnOther.Add(replica, new Dictionary<string, OtherReplicaTuple>());
                processingOnOther[replica].Add(t.ID, new OtherReplicaTuple(t));
                Monitor.Pulse(processingOnOther);
                Monitor.Exit(processingOnOther);
            }
/* TODO: Remove
Console.WriteLine("\t\t\t\tCONFIRMED ELECTION!!!");
Console.WriteLine("\t\t\t\tTUPLE CONTENT: <" + string.Join(" - ", t.Tuple) + ">");
Console.WriteLine("\t\t\t\tTUPLE ID: " + t.ID);
*/
        }

        public void finishedProcessing(string tupleID, List<TupleWrapper> result, string url) {
            while (frozen)
                Thread.Sleep(1);

            Monitor.Enter(processingOnOther);
/* TODO: Remove
foreach (KeyValuePair<string, Dictionary<string, OtherReplicaTuple>> entry in processingOnOther)
Console.WriteLine("\t\tKEY OF OTHER REPLICA TUPLES: " + entry.Key);
Console.WriteLine("\t\tKEY RECEIVED: " + url);
*/
            Dictionary<string, OtherReplicaTuple> t = processingOnOther[url];
            Monitor.Pulse(processingOnOther);
            Monitor.Exit(processingOnOther);
            t[tupleID].finishedProcessing(result);
        }

        public void finishedSending(string tupleID, string url) {
            while (frozen)
                Thread.Sleep(1);

            Monitor.Enter(seenTuples);
            seenTuples.Add(tupleID);
            Monitor.Pulse(seenTuples);
            Monitor.Exit(seenTuples);

            Monitor.Enter(processingOnOther);
            Dictionary<string, OtherReplicaTuple> t = processingOnOther[url];
            t.Remove(tupleID);
            Monitor.Pulse(processingOnOther);
            Monitor.Exit(processingOnOther);
        }

        public bool ping(string originUrl,bool sameLayer) {

            while (frozen) {
                Thread.Sleep(100);

            }
            /* if (sameLayer) {
                 if (!otherReplicasURL.Contains(originUrl)) 
                     otherReplicasURL.Add(originUrl);
             }*/
            //Console.WriteLine("Ping requested by{0} in {1}", originUrl, replicaAddress);
            return true;
        }

        public void failureDetector() {
            while (true) {
                Thread.Sleep(5000);
                List<string> otherReplicasURLcopy = otherReplicasURL.ToList();
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
                        var completed = thread.Join(5000);
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

        private void handleCrashedReplica(string url) {
            Console.WriteLine("!!!handleCrashedReplica({0})", url);
            lock (otherReplicasURL) {
                otherReplicasURL.Remove(url);//TODO CHECK if we can do this !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }


        }//TODO
        private void handleSlowReplica(string url) {
            Console.WriteLine("!!!handleSlowReplica({0})", url);
                }//TODO


        /********************
         * AGREEMENT THREAD *
         *******************/
        private void chooseProcessingReplica(TupleWrapper t) {
            List<string> toConfirm = new List<string>();

            // TODO: Include the current replica's URL in the list 
            // TODO: Use the majority of the replicas
            // TODO: Include the address of the current replica in the same list, all lists of replicas should be in the same order
            // Must be done on himself
            toConfirm.Add(replicaAddress);
            if (!tryElectionOfProcessingReplica(t, replicaAddress))
                return;

            foreach (string url in otherReplicasURL) {
                ReplicaInterface r;

                // TODO: Confirm if this will be a problem!!!
                if ((r = getReplica(url)) == null)
                    continue;

                if (!r.tryElectionOfProcessingReplica(t, replicaAddress))
                    return;

                toConfirm.Add(url);
            }

            foreach(string replica in toConfirm) {
                ReplicaInterface r;

                // TODO: Confirm if this will be a problem!!!
                if ((r = getReplica(replica)) == null)
                    continue;
                r.confirmElection(t);
            }
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
