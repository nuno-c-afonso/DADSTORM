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

        public void sendToNext(TupleWrapper tuple) {
            if (nextOperator.Count != 0) {
                ReplicaInterface replica = null;
                string outputReplica = calculateNext(tuple.Tuple);

                try {
                    replica = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), outputReplica);
                } catch (System.Net.Sockets.SocketException e) {
                    Console.WriteLine("Error with host " + outputReplica);
                    Console.WriteLine(e);
                }

                try {
                    // TODO FIXME something not right with tuple on mylib.dll version of professors
                    semantics.sendTuple(replica, tuple);
                }
                catch (System.Net.Sockets.SocketException) { // The replica is dead
                    Console.WriteLine("  ##!! "+ outputReplica + " was down, removed from nextOperator list. Resending !!##\n");

                    nextOperator.Remove(outputReplica);
                    sendToNext(tuple);
                } catch(CouldNotSendTupleException) { // The replica is alive, but slow
                    Console.WriteLine("  ##!! " + outputReplica + " did not respond in time. Resending !!##\n");
                    sendToNext(tuple);
                } catch(Exception) { }
            }
            //ELSE can write on file

        }

        // To be implemented by subclasses
        public abstract string calculateNext(string[] tuple);
    }
}