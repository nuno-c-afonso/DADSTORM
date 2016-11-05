using CommonClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public interface Semantics {
        void sendTuple(ReplicaInterface replica, string[] tuple);
    }
}
