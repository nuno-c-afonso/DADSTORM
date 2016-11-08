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
            string operationS = ""; 
            foreach(string s in op) {
                operationS += s.Replace("\"", "\\\"").Replace(" ", "\\ ") + " " ;
            }
            process.StartInfo.Arguments += " -op " + operationS;
            /*foreach (string s in op)
                process.StartInfo.Arguments += " " + s;*/
                        Console.Write(" " + process.StartInfo.Arguments);
            process.StartInfo.Arguments += " -r " + repIndex + " " + string.Join(" ", replicas);
            process.StartInfo.Arguments += " -o " + string.Join(" ", output); //#FIXME if output is empty "-op " is sent alst space may cause problems
            process.StartInfo.Arguments += " -i " + string.Join(" ", input);
            process.Start();
        }

        private void addListString(List<string> l, string final) {
            foreach (string s in l)
                final += " " + s;
        }
    }
}
