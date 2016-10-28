using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManipulator {
    public class ReadFileByLine {
        private string[] lines;
        private int currentLine = 0;

        public ReadFileByLine(string filepath) {
            lines = System.IO.File.ReadAllLines(filepath);
        }

        public string nextLine() {
            if (currentLine < lines.Length)
                return lines[currentLine++];
            throw new EOFException();
        }

        public string nextValidLine()
        {
            try
            {
                string line = nextLine();
                LineParser ln = new LineParser(line);
                //var words = ln.Words;
                return line;
            }
            catch (EOFException)
            {
                throw;
            }
            catch (Exception ex) when (ex is EmptyLineException || ex is LineIsCommentException)

            {
                return nextValidLine();
            }

            throw new EOFException();
        }


    }
}
