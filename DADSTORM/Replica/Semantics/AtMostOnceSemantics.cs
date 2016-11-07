using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonClasses;
using System.Runtime.Remoting.Messaging;

namespace Replica {
    public class AtMostOnceSemantics : Semantics {

        public delegate void RemoteAsyncDelegate(string[] tuple);

        // This is the call that the AsyncCallBack delegate will reference.
        public static void remoteAsyncCallBack(IAsyncResult ar) {
            RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            Console.WriteLine("The asynchronous call was successful!");
        }

        // TODO: Check if this is the right asynchronous call
        public void sendTuple(ReplicaInterface replica, string[] tuple) {
            RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(replica.addTuple);
            IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, null, replica);

            /* // Asynchronous call
             RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(replica.addTuple);
             // Create delegate to local callback
             AsyncCallback RemoteCallback = new AsyncCallback(remoteAsyncCallBack);
             // Call remote method
             IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, RemoteCallback, replica);*/
        }
    }
}
