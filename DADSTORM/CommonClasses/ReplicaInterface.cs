using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses {
    public interface ReplicaInterface {
        void addTuple(string[] tuple);
        void Start();
        void Interval(int time);
        string Status();
        void Crash();
        void Freeze();
        void Unfreeze();

        bool wasElementSeen(string s);

        int numberOfProcessedTuples();
    }
}
