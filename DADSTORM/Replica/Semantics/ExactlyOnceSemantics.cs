using System;
using CommonClasses;

namespace Replica {
    public class ExactlyOnceSemantics : Semantics {
        // The different mechanism will be done upon receiving the tuple
        AtLeastOnceSemantics alos = new AtLeastOnceSemantics();

        public override void sendTuple(ReplicaInterface replica, TupleWrapper tuple) {
            alos.sendTuple(replica, tuple);
        }
    }
}