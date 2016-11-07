using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class UniqOperation : GlobalStateOperation {
        private HashSet<string> seenStrings;
        private int fieldIndex;
        public UniqOperation(List<string> replicasURL, int myselfIndex, int fieldNumber)
            : base(replicasURL, myselfIndex) {
            this.fieldIndex = fieldNumber - 1;
            seenStrings = new HashSet<string>();
        }

        public override bool wasElementSeen(string s) {
            bool contains = seenStrings.Contains(s);

            if (!contains)
                seenStrings.Add(s);

            return contains;
        }

        private void processedTuple(string[] tuple) {
            foreach (string s in tuple)
                seenStrings.Add(s);
        }

        public override List<string[]> Operate(string[] tuple) {
            string element = tuple[fieldIndex];

            if (wasElementSeen(element))
                return null;

            processedTuple(tuple);

            bool wasInOther = false;
            foreach(string s in OtherReplicas) {
                ReplicaInterface rep = getGeneralReplica(s);
                if (wasInOther = rep.wasElementSeen(element))
                    break;
            }

            if (wasInOther)
                return null;

            List<string[]> l = new List<string[]>();
            l.Add(tuple);
            return l;
        }
    }
}
