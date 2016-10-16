using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CommonClasses {
    public class ProcessCreator : MarshalByRefObject, IProcessCreator {
        public void createReplica(string URL, string routing, string op, List<string> inputs, List<string> output) {
            
            // TODO: Launch a new Replica with the given arguments
            // TODO: Receive the Puppet Master's URL, in order to send the logging info to the port 10001
            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Replica\\bin\\Debug\\Replica.exe";

            // Building the arguments for the main
            process.StartInfo.Arguments = URL + " " + routing + " " + op + " -i";
            addListString(inputs, process.StartInfo.Arguments);
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
