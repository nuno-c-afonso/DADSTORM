using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CommonClasses {
    public class ProcessCreator : MarshalByRefObject, IProcessCreator {
        public void createReplica(string masterURL, string routing, string semantics, string logLevel,
            int repIndex, List<string> op, List<string> replicas, List<string> output) {
            
            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Replica\\bin\\Debug\\Replica.exe";

            // Building the arguments for the main
            process.StartInfo.Arguments = masterURL + " " + routing + " "
                + semantics + " " + logLevel + " -op";
            addListString(op, process.StartInfo.Arguments);
            process.StartInfo.Arguments += " -r " + repIndex;
            addListString(replicas, process.StartInfo.Arguments);
            process.StartInfo.Arguments += " -o";
            addListString(output, process.StartInfo.Arguments);

            process.Start();
        }

        private void addListString(List<string> l, string final) {
            foreach (string s in l)
                final += " " + s;
        }
    }
}
