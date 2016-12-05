using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class TupleWrapper {
        private int[] id;
        private string[] tuple;

        public int[] ID {
            get {
                return ID;
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
