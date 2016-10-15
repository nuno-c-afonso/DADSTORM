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

            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Replica\\bin\\Debug\\Replica.exe";
            process.StartInfo.Arguments = "-n";
            //process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            process.Start();
            process.WaitForExit();// Waits here for the process to exit.
        }
    }
}
