using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using System.Diagnostics;

namespace FileManipulator {
    // diferenciar configCommands e operatorCommands


    public class ReadFileByLineFiltered {
        private string[] lines;
        private int currentLine = 0;
        private int operatorCommandFirstLine = 0;

        public ReadFileByLineFiltered(string filepath) {
            int i;
            lines = System.IO.File.ReadAllLines(filepath);
            lines = lines.Where(line => (line.Length > 0 && line[0] != '%')).ToArray();

            // find where operatorCommands start
            for (i = 0; i < lines.Length; i++) {
                if (Shell.doesCommandExist(lines[i])) {
                    break;
                }
            }

            operatorCommandFirstLine = i;
        }

        public string nextLine() {
            if (currentLine < lines.Length)
                return lines[currentLine++];
            throw new EOFException();
        }

        public List<string> linesBetweenIndexes(int lower, int max)
        {
            List<string> l = new List<string>();
            for (int i = lower; i < max; i++)
            {
                l.Add(lines[i]);
            }
            return l;
        }

        public List<string> remainingLines()
        {
            return linesBetweenIndexes(currentLine, lines.Length);
        }

        public List<string> getConfigCommandsLines()
        {
            //should the line change be here ? #TODO
            if (currentLine < operatorCommandFirstLine)
                currentLine = operatorCommandFirstLine;

            return linesBetweenIndexes(0, operatorCommandFirstLine);
        }

        public List<string> getOperatorsCommandsLines()
        {

            return linesBetweenIndexes(operatorCommandFirstLine, lines.Length);
        }


    }
}
