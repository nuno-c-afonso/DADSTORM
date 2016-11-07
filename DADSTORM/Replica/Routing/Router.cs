using CommonClasses;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Replica {
    public abstract class Router {
        private List<string> nextOperator;
        private Semantics semantics;

        public List<string> NextOperator {
            get {
                return nextOperator;
            }
        }

        public Router(List<string> output, string semantics) {
            Console.WriteLine("3-Router Created");
            nextOperator = output;
            string lowerCase = semantics.ToLower();

            if (lowerCase.Equals("at-most-once"))
                this.semantics = new AtMostOnceSemantics();
            else if (lowerCase.Equals("at-least-once"))
                this.semantics = new AtLeastOnceSemantics();
            else
                this.semantics = new ExactlyOnceSemantics();
        }

        public void sendToNext(string[] tuple) {
            ReplicaInterface replica = null;
            string outputReplica = calculateNext(tuple);

            try {
                replica = (ReplicaInterface) Activator.GetObject(typeof(ReplicaInterface), outputReplica);
            }
            catch (System.Net.Sockets.SocketException e) {
                Console.WriteLine("Error with host " + outputReplica);
                Console.WriteLine(e);
            }

            semantics.sendTuple(replica, tuple);
        }

        // To be implemented by subclasses
        public abstract string calculateNext(string[] tuple);
    }
}