using System;
using CommonClasses;

namespace Replica {
    public class ExactlyOnceSemantics : Semantics {
        public override void sendTuple(ReplicaInterface replica, TupleWrapper tuple) {
            // TODO
            throw new NotImplementedException();
        }
    }
}