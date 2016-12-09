using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses {
    public class DTOtupleState {
        public static int RECEIVED = 0;
        public static int ATRIBUTED = 1;
        public static int PROCESSED = 2;
        public static int SEND = 3;
        public static int DONTHAVE = 4;

        public int stage = 100;
        public OtherReplicaTuple tuple = null;
        public TupleWrapper tupleWraper = null;
        public string id = null;
        public string owner;

        ///<resume>stat when have no owner</resume>
        public DTOtupleState(TupleWrapper tupleWraper) {
            stage = RECEIVED;
            this.tupleWraper = tupleWraper;
        }

        ///<resume>when is atributed to the replica itself</resume>
        public DTOtupleState(TupleWrapper tupleWraper, string owner) {
            stage = ATRIBUTED;
            this.tupleWraper = tupleWraper;
        }

        ///<resume>when is atributed to other replica itself</resume>
        public DTOtupleState(OtherReplicaTuple tuple, string owner) {
            if (tuple.isProcessed())
                stage = PROCESSED;
            else
                stage = ATRIBUTED;

            this.tuple = tuple;
            this.owner = owner;                      
        }

        ///<resume>when is already send</resume>
        public DTOtupleState(string id) {
            this.id = id;
            stage = SEND;
        }

        ///<resume>when dont have it</resume>
        public DTOtupleState() {
            stage = DONTHAVE;
        }




    }
}
