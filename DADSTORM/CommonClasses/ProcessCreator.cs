using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace CommonClasses {
    public class ProcessCreator : MarshalByRefObject, IProcessCreator {
        public void createReplica(string masterURL, string routing, string incomingRouting, string semantics, string logLevel,
            int repIndex, List<string> op, List<string> replicas, List<string> output, List<string> input) {


            foreach (string s in  op)
                Console.Write(" " + s);//FIXME REMOVE DIOGO
           Console.WriteLine("");
            


            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Replica\\bin\\Debug\\Replica.exe";

            // Building the arguments for the main
            process.StartInfo.Arguments =  masterURL + " " + routing + " " + incomingRouting + " " + semantics + " " + logLevel;
            string operations = ""; 
            foreach(string s in op) {
                operations += s.Replace("\"", "\\\"").Replace(" ", "\\ ") + " " ;
            }
            process.StartInfo.Arguments += " -op " + operations;
            process.StartInfo.Arguments += " -r " + repIndex + " " + string.Join(" ", replicas);
            process.StartInfo.Arguments += " -o " + string.Join(" ", output); //#FIXME if output is empty "-op " is sent and space may cause problems
            process.StartInfo.Arguments += " -i " + string.Join(" ", input);
            process.Start();
        }
    }
}
