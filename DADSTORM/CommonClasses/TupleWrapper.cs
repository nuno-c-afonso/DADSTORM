using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses {
    [Serializable]
    public class TupleWrapper {
        private string id;
        private string[] tuple;

        // TODO: Check when the replica reads from a file and these tuples are new!!!
        public override bool Equals(Object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            TupleWrapper t = (TupleWrapper) obj;
            return this.id.Equals(t.id);
        }

        public override int GetHashCode() {
            int INIT_FACTOR = 23;
            int MUL_FACTOR = 41;

            int hash = INIT_FACTOR;
            hash = hash * MUL_FACTOR + id.GetHashCode();
            return hash * MUL_FACTOR + tuple.GetHashCode();
        }

        public string ID {
            get {
                return id;
            }
        }

        public string[] Tuple {
            get {
                return tuple;
            }
        }

        public TupleWrapper(string previousID, string replicaID, string[] tuple) {
            id = previousID + replicaID + "|";
            this.tuple = tuple;
        }
    }
}
