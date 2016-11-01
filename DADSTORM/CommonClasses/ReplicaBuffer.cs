using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonClasses
{
    public class ReplicaBuffer : MarshalByRefObject, ReplicaInterface {

        private Queue tupleQueue;
        bool start = false;
        int waitingTime = 0;
        bool statusRequested = false;//check this
        bool crashed = false;
        object freezed = false;

        public int WaitingTime{
            get { return waitingTime; }
        }

        public bool Crashed{
            get { return crashed; }
        }

        public bool StatusRequested{
            get{
                if (statusRequested)
                {
                    statusRequested = false;
                    return true;
                }
                else
                    return false;
            }
        }




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
            lock (tupleQueue.SyncRoot)
            {
                if (tupleQueue.Count == 0)
                    Monitor.Wait(tupleQueue.SyncRoot);
            }

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
            waitingTime = time;
        }

        //Command to print the current status
        //USED BY:PuppetMaster
        public string Status() {
            statusRequested = true; //TODO how to do this?
            throw new NotImplementedException();
        }

        //Command to simulate a program crash
        //USED BY:PuppetMaster 
        public void Crash() {
            crashed = true;
        }

        //Command used to simulate slow server
        //USED BY:PuppetMaster
        public void Freeze() {
            freezed = true;
        }

        //Command used to end the slow server simulation
        //USED BY:PuppetMaster
        public void Unfreeze(){
            freezed = false;
            Monitor.PulseAll(freezed);
        }




        //if the freeze is true who call this will wait until it is unfreezed
        //USED BY: buffer consumer
        public void checkFreeze() {
            lock (freezed)
            {
                while ((bool)freezed == true)
                    Monitor.Wait(freezed);
                Monitor.Pulse(freezed);
            }


        }

        //if the freeze is true who call this will wait until it is unfreezed
        //USED BY: owner(replica)



        public bool wasElementSeen(string s) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
