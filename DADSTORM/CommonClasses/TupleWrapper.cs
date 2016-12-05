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
        public static bool operator ==(TupleWrapper t1, TupleWrapper t2) {
            return t1.ID.Equals(t2.ID);
        }

        public static bool operator !=(TupleWrapper t1, TupleWrapper t2) {
            return !(t1 == t2);
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
