using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replica {
    public class OutputOperation : Operation {
        public override List<string[]> Operate(string[] tuple) {
            string outputFile = @".\output.txt";

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(outputFile, true)) {
                foreach (string line in tuple) {

                    // If the line doesn't contain the word 'Second', write the line to the file.
                    file.WriteLine(line);
                }
            }

            List<string[]> result = new List<string[]>();

            result.Add(tuple);
            return null;
        }
    }
}
