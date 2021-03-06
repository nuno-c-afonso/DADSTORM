﻿using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public abstract class GlobalStateOperation : Operation {
        private List<string> otherReplicas = new List<string>();

        public GlobalStateOperation(List<string> replicasURL, int myselfIndex) {
            int numReplicas = replicasURL.Count;
            for (int i = 0; i < numReplicas; i++)
                if (i != myselfIndex)
                    otherReplicas.Add(replicasURL[i]);
        }

        public List<string> OtherReplicas {
            get {
                return otherReplicas;
            }
        }

        protected ReplicaInterface getGeneralReplica(string url) {
            ReplicaInterface obj = null;

            try {
                obj = (ReplicaInterface)Activator.GetObject(typeof(ReplicaInterface), url);
            }
            catch (System.Net.Sockets.SocketException e) {
                Console.WriteLine("Error with host " + url);
                Console.WriteLine(e);
            }

            return obj;
        }
    }
}
