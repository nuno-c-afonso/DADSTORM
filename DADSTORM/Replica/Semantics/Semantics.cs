using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public abstract class Semantics {
        // It is measured in seconds
        public static int TIMEOUT_VALUE = 5;

        // Used for the asynchronous calls
        public delegate void RemoteAsyncDelegate(TupleWrapper tuple);

        public abstract void sendTuple(ReplicaInterface replica, TupleWrapper tuple);
    }
}
