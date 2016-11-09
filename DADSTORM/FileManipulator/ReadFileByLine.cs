using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManipulator {
    public class ReadFileByLine {
        private string[] lines;
        private int currentLine = 0;

        public string[] Lines {
            set {
                lines = value;
            }

            get {
                return lines;
            }
        }

        public int CurrentLine {
            set {
                currentLine = value;
            }

            get {
                return currentLine;
            }
        }

        public ReadFileByLine(string filepath) {
            lines = System.IO.File.ReadAllLines(filepath);
        }

        public string nextLine() {
            if (currentLine < lines.Length)
                return lines[currentLine++];
            throw new EOFException();
        }
    }
}
