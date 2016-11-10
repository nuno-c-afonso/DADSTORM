using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using System.Diagnostics;

namespace FileManipulator {
    
    // To differentiate between configCommands and operatorCommands
    public class ReadFileByLineFiltered : ReadFileByLine {
        private int operatorCommandFirstLine = 0;

        public ReadFileByLineFiltered(string filepath) : base(filepath) {
            int i;

            Lines = Lines.Where(line => (line.Length > 0 && line[0] != '%')).ToArray();

            // find where operatorCommands start
            for (i = 0; i < Lines.Length; i++) {
                if (Shell.doesCommandExist(Lines[i])) {
                    break;
                }
            }

            operatorCommandFirstLine = i;
        }

        public List<string> linesBetweenIndexes(int lower, int max) {
            List<string> l = new List<string>();
            for (int i = lower; i < max; i++) {
                l.Add(Lines[i]);
            }
            return l;
        }

        public List<string> remainingLines() {
            return linesBetweenIndexes(CurrentLine, Lines.Length);
        }

        public List<string> getConfigCommandsLines() {
            if (CurrentLine < operatorCommandFirstLine)
                CurrentLine = operatorCommandFirstLine;

            return linesBetweenIndexes(0, operatorCommandFirstLine);
        }

        public List<string> getOperatorsCommandsLines() {
            return linesBetweenIndexes(operatorCommandFirstLine, Lines.Length);
        }


    }
}
