using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace CommonClasses {
    public class ProcessCreator : MarshalByRefObject, IProcessCreator {
        public void createReplica(string masterURL, string routing, string semantics, string logLevel,
            int repIndex, List<string> op, List<string> replicas, List<string> output, List<string> input) {


            foreach (string s in input){//FIXME REMOVE DIOGO
                Console.Write(" " + s);//FIXME REMOVE DIOGO
                Console.WriteLine("");
            }
            Console.WriteLine(Directory.GetCurrentDirectory());

            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Replica\\bin\\Debug\\Replica.exe";

            // Building the arguments for the main
            process.StartInfo.Arguments =  masterURL + " " + routing + " " + semantics + " " + logLevel;
            process.StartInfo.Arguments += " -op " + string.Join(" ", op);
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
