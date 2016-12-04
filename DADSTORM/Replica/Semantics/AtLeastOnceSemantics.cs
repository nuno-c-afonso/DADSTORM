using System;
using CommonClasses;
using System.Threading;

namespace Replica {
    public class AtLeastOnceSemantics : Semantics {
        public override void sendTuple(ReplicaInterface replica, string[] tuple) {
            RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(replica.addTuple);
            IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, null, replica);

            // TODO: Check if this can used
            bool completed = false;
            for (int counter = 0; counter < TIMEOUT_VALUE && !(completed = RemAr.IsCompleted); counter++)
                Thread.Sleep(1);

            if (!completed)
                throw new CouldNotSendTupleException();
        }
    }
}