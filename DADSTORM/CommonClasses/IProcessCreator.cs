using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses
{
    public interface IProcessCreator {
        void createReplica(string URL, string routing, string op, List<string> inputs, List<string> output);
    }
}
