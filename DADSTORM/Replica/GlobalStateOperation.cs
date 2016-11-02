using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public abstract class GlobalStateOperation : Operation {
        TcpChannel channel;
        private List<string> otherReplicas = new List<string>();

        public GlobalStateOperation(List<string> replicasURL, int myselfIndex) {
            int numReplicas = replicasURL.Count;
            for (int i = 0; i < numReplicas; i++)
                if (i != myselfIndex)
                    otherReplicas.Add(replicasURL[i]);

            channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);
        }

        public List<string> OtherReplicas {
            get {
                return otherReplicas;
            }
        }

        // TODO: Check if is possible to do this without the channel closing
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
