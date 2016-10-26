using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonClasses
{
    public class ReplicaBuffer : MarshalByRefObject, ReplicaInterface {

        private Queue tupleQueue;
        bool start = false;
        int interval = 0;
        bool statusRequested = false;//check this
        bool crashed = false;
        public bool Crashed{
            get { return crashed; }//see locks
        }
        object freezed = false;


        public ReplicaBuffer(): base(){
            tupleQueue = new Queue();
        }

        //method used to send tuples to the owner of this buffer
        //USED BY: other replicas, input file reader
        public void addTuple(string[] tuple) {
            lock(tupleQueue.SyncRoot);
            tupleQueue.Enqueue(tuple);
            Monitor.Exit(tupleQueue.SyncRoot);
            Monitor.Pulse(tupleQueue.SyncRoot);
        }

        //method used to get tuples from the buffer
        //USED BY: owner(replica)
        public string[] getTuple()
        {
            lock(tupleQueue.SyncRoot);
            if (tupleQueue.Count == 0)
                Monitor.Wait(tupleQueue.SyncRoot);
            string[] result =(String[])tupleQueue.Dequeue();
            Monitor.Exit(tupleQueue.SyncRoot);
            Monitor.Pulse(tupleQueue.SyncRoot);
            return result;
        }




        //Command to start working
        //USED BY:PuppetMaster
        public void Start() {
            start = true;
        }

        //Command  to set the time to wait betwen tuple processing
        //USED BY:PuppetMaster
        public void Interval(int time) {
            interval = time;
        }

        //Command to print the current status
        //USED BY:PuppetMaster
        public string Status() {
            statusRequested = true;
            return "FIXME"; }//TODO how to do this?

        //Command to simulate a program crash
        //USED BY:PuppetMaster 
        public void Crash() {
            crashed = true;
        }

        //Command used to simulate slow server
        //USED BY:PuppetMaster
        public void Freeze() {
            lock(freezed);
            freezed = true;
            Monitor.Exit(freezed);
            Monitor.PulseAll(freezed);
        }

        //Command used to end the slow server simulation
        //USED BY:PuppetMaster
        public void Unfreeze(){
            lock(freezed);
            freezed = false;
            Monitor.Exit(freezed);
            Monitor.PulseAll(freezed);
        }


        //if the freeze is true who call this will wait until it is unfreezed
        //USED BY: owner(replica)
        public void checkFreeze() {
            lock(freezed);
            while ((bool)freezed == true)
                Monitor.Wait(freezed);
            Monitor.Exit(freezed);
            Monitor.Pulse(freezed);
        }





    }
}
