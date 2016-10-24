﻿using System;
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
        bool freezed = false;

        public ReplicaBuffer(): base(){
            tupleQueue = new Queue();
        }

        //TODO make methods
        public void addTuple(string[] tuple) {
            lock(tupleQueue.SyncRoot);
            tupleQueue.Enqueue(tuple);
            Monitor.Exit(tupleQueue.SyncRoot);
            Monitor.Pulse(tupleQueue.SyncRoot);
        }

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



        
        public void Start() {
            start = true;
        }
        public void Interval(int time) {
            interval = time;
        }
        public string Status() {
            statusRequested = true;
            return "FIXME"; }//TODO how to do this?
        public void Crash() {
            crashed = true;
        }
        public void Freeze() {
            freezed = true;
        }
        public void Unfreeze(){
            freezed = false;
        }


        public bool simulateCrash() {
            return crashed;
        }




    }
}
