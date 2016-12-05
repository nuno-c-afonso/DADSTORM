using System;
using CommonClasses;
using System.Threading;

namespace Replica {
    public class AtLeastOnceSemantics : Semantics {
        public override void sendTuple(ReplicaInterface replica, TupleWrapper tuple) {
            RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(replica.addTuple);
            IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, null, replica);

            int counter;
            for (counter = 0; counter < TIMEOUT_VALUE && !RemAr.IsCompleted; counter++)
                Thread.Sleep(1);

            if (counter == TIMEOUT_VALUE)
                throw new CouldNotSendTupleException();
        }
    }
}