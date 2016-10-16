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
            process.StartInfo.Arguments = "-n";
            process.Start();
        }
    }
}
