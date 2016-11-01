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
        private HashSet<string> seenStrings = new HashSet<string>();
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

        public bool wasElementSeen(string s) {
            bool contains = seenStrings.Contains(s);

            if (!contains)
                seenStrings.Add(s);

            return contains;
        }

        protected void processedTuple(string[] tuple) {
            foreach (string s in tuple)
                seenStrings.Add(s);
        }

        // TODO: Check if is possible to do this without the channel closing
        // FIXME: The object is a replica not an operation
        protected Operation getGeneralReplica(string url) {
            Operation obj = null;

            try {
                obj = (Operation)Activator.GetObject(typeof(Operation), url);
            }
            catch (System.Net.Sockets.SocketException e) {
                Console.WriteLine("Error with host " + url);
                Console.WriteLine(e);
            }

            return obj;
        }

        public abstract List<string[]> Operate(string[] tuple);
    }
}
