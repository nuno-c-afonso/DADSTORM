using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses
{
    public interface IProcessCreator {
        void createReplica(string masterURL, string routing, string op,
            string semantics, string logLevel, List<string> replicas, List<string> output);
    }
}
