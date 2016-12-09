using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class CountOperation : GlobalStateOperation {
        private int counter;

        public CountOperation(List<string> replicasURL, int myselfIndex)
            : base(replicasURL, myselfIndex) {
            counter = 0;
        }

        public override List<string[]> Operate(string[] tuple) {
            int globalCounter = ++counter;

            foreach(string url in OtherReplicas) {
                try {
                    ReplicaInterface r = getGeneralReplica(url);
                    globalCounter += r.numberOfProcessedTuples();
                } catch (System.Net.Sockets.SocketException) { }
            }

            List<string[]> l = new List<string[]>();
            string[] tupleOut = { globalCounter.ToString() };
            l.Add(tupleOut);
            return l;
        }

        public override int numberOfProcessedTuples() {
            return counter;
        }
    }
}
