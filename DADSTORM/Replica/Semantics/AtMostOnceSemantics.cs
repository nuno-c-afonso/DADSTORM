using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonClasses;
using System.Runtime.Remoting.Messaging;

namespace Replica {
    public class AtMostOnceSemantics : Semantics {
        public override void sendTuple(ReplicaInterface replica, string[] tuple) {
            RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(replica.addTuple);
            IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, null, replica);
        }
    }
}
