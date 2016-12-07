using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonClasses;

namespace Replica {
    public class OtherReplicaTuple {
        private static int RECEIVED = 0;
        private static int PROCESSED = 1;

        private TupleWrapper tuple;
        private int state = RECEIVED;
        private List<TupleWrapper> result = null;

        public OtherReplicaTuple(TupleWrapper t) {
            tuple = t;
        }

        public void finishedProcessing(List<TupleWrapper> l) {
            result = l;
            state = PROCESSED;
        }
    }
}
