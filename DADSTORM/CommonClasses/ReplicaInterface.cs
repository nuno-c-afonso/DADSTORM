using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses {
    public interface ReplicaInterface {
        void addTuple(TupleWrapper tuple);
        void Start();
        void Interval(int time);
        void Status();
        void Crash();
        void Freeze();
        void Unfreeze();

        bool wasElementSeen(string s);
        int numberOfProcessedTuples();

        // Used in fault-tolerance
        void arrivedTuple(TupleWrapper t);
        void tryElectionOfProcessingReplica(TupleWrapper t, string url);
        void confirmElection(TupleWrapper t, string url);
        void finishedProcessing(string tupleID, List<TupleWrapper> result, string url);
        void finishedSending(TupleWrapper tuple, string url);
        bool ping(string originUrl, bool sameLayer);


    }
}
