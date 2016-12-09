using System;
using CommonClasses;
using System.Threading;

namespace Replica {
    public class AtLeastOnceSemantics : Semantics {
        public override void sendTuple(ReplicaInterface replica, TupleWrapper tuple) {

            Console.WriteLine("\n Inside sendTuple");
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    replica.addTuple(tuple);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });

            thread.Start();

            if (!thread.Join(TIMEOUT_VALUE))
            {
                Console.WriteLine("####### didnt finish call in 5 seconds");

                thread.Abort();
                //throw new TimeoutException();
                throw new CouldNotSendTupleException();
            }

            if (exception != null)
            {
                throw exception;
            }

        }
    }
}