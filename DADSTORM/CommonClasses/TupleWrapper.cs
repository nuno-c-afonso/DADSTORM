using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses {
    [Serializable]
    public class TupleWrapper {
        private int[] id;
        private string[] tuple;

        // TODO: Include the replica's index in the ID
        public static bool operator ==(TupleWrapper t1, TupleWrapper t2) {
            int size = t1.ID.Length;

            if (size == t2.ID.Length) {
                for (int i = 0; i < size; i++) {
                    if (t1.ID[i] != t2.ID[i])
                        return false;
                }

                return true;
            }

            return false;
        }

        public static bool operator !=(TupleWrapper t1, TupleWrapper t2) {
            return !(t1 == t2);
        }

        public int[] ID {
            get {
                return id;
            }
        }

        public string[] Tuple {
            get {
                return tuple;
            }
        }

        public TupleWrapper(int[] previousID, int replicaID, string[] tuple) {
            int previousSize = previousID.Length;
            id = new int[previousSize + 1];
            previousID.CopyTo(id, 0);
            id[previousSize] = replicaID;
            this.tuple = tuple;
        }
    }
}
