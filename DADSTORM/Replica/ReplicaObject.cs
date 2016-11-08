using CommonClasses;
using System;
using System.Collections;
using System.Collections.Generic;
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
        
        private bool start = false;
        private int waitingTime = 0;
        private bool freezed = false;

        private Thread consumer;

        public bool Started {
            get { return start; }
        }

        public ReplicaObject(string PuppetMasterUrl, string routing, string semantics,
            string logLevel, Operation operation, List<string> output, string replicaAddress, string operationName) {
            tupleQueue = new Queue();

            this.PuppetMasterUrl = PuppetMasterUrl;
            this.logLevel = logLevel.Equals("full");
            this.operation = operation;
            this.replicaAddress = replicaAddress;
            this.operationName = operationName;

            string routingLower = routing.ToLower();
            char[] delimiters = { '(', ')' };
            string[] splitted = routingLower.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if (splitted[0].Equals("primary"))
                router = new PrimaryRouter(output, semantics);
            else if (splitted[0].Equals("random"))
                router = new RandomRouter(output, semantics);
            else
                router = new HashRouter(output, semantics, int.Parse(splitted[1]));

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

        public void addThread(Thread t) {
            consumer = t;
        }

        //method used to send tuples to the owner of this buffer
        //USED BY: other replicas, input file reader
        public void addTuple(string[] tuple) {
            Console.WriteLine("addTuple({0})", tuple);
            Monitor.Enter(tupleQueue.SyncRoot);
            tupleQueue.Enqueue(tuple);
            Monitor.PulseAll(tupleQueue.SyncRoot);
            Monitor.Exit(tupleQueue.SyncRoot);
        }

        //method used to get tuples from the buffer
        //USED BY: owner(replica)
        public string[] getTuple() {
            Console.WriteLine("getTuple()");
            Monitor.Enter(tupleQueue.SyncRoot);
            while(tupleQueue.Count == 0)
                    Monitor.Wait(tupleQueue.SyncRoot);
            string[] result = (String[]) tupleQueue.Dequeue();
            Console.WriteLine("         GOT tuple");
            Monitor.Pulse(tupleQueue.SyncRoot);
            Monitor.Exit(tupleQueue.SyncRoot);
            return result;
        }

        //Command to start working
        //USED BY:PuppetMaster
        public void Start() {
            Console.WriteLine("-->START comand received");
            start = true;
        }

        //Command  to set the time to wait betwen tuple processing
        //USED BY:PuppetMaster
        public void Interval(int time) {
            Console.WriteLine("-->Interva comand received time={0}",time);
            waitingTime = time;
        }

        private void testLog() {
            try {
                //log.Log("Status " + operationName + " " + replicaAddress); //  + " " + IPAddresses.LocalIPAddress()
                Console.WriteLine("Replica Address: " + replicaAddress + "\r\nOperation: " +
                    operationName + "\r\nFLAGS\r\nStart: " + start + "\r\nWaiting: " +
                    waitingTime + "\r\nFreezed: " + freezed);
            } catch (Exception ex) {
                Console.WriteLine("second ex: " + ex);
            }
        }

        //Command to print the current status
        //USED BY:PuppetMaster
        public void Status() {
            Console.WriteLine("-->STATUS command received");

            new Thread(() => testLog()).Start();
            //Thread t = new Thread(() => testLog());
            //t.Start();
            //Console.WriteLine("after thread\n-----------");
            //testLog();
        }

        //Command to simulate a program crash
        //USED BY:PuppetMaster 
        public void Crash() {
            consumer.Abort();
        }

        //Command used to simulate slow server
        //USED BY:PuppetMaster
        public void Freeze() {
            Console.WriteLine("-->FREEZE comand received");
            freezed = true;
        }

        //Command used to end the slow server simulation
        //USED BY:PuppetMaster
        public void Unfreeze() {
            Console.WriteLine("-->UNFREEZE comand received");
                freezed = false;
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
            Console.WriteLine("6-Waiting for START comand");
            while (!start)
                Thread.Sleep(100);

            while (true) {
                //see if it is feezed
                while(freezed == true)
                    Thread.Sleep(100);

                //wait the defined time between processing
                Thread.Sleep(waitingTime);

                //get tuple from the buffer
                string[] tuple = getTuple();

                List<string[]> result = operation.Operate(tuple);
                if (result != null) {
                    foreach (string[] outTuple in result){
                        Console.WriteLine("sending tuple");
                        router.sendToNext(outTuple);

                        if (logLevel)
                            log.Log("tuple " + operationName + " " + replicaAddress + " <" + string.Join(" - ", outTuple) + ">" );
                    }
                }
            }
        }
    }
}
