using System;
using CommonClasses;
using System.Threading;

namespace Replica {
    public class AtLeastOnceSemantics : Semantics {
        public override void sendTuple(ReplicaInterface replica, TupleWrapper tuple) { 
            Thread t = new Thread(() => replica.addTuple(tuple));
            t.Start();
            if (!t.Join(TimeSpan.FromSeconds(TIMEOUT_VALUE))) {
                t.Abort();
                throw new CouldNotSendTupleException();
            }
           
        }
    }
}