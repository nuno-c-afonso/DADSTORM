using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManipulator {
    public class LineParser {
        private string line;
        private ReadOnlyCollection<string> words;

        public LineParser(string line) {
            if (line == null || line.Equals(""))
                throw new EmptyLineException();

            this.line = line;
        }

        public ReadOnlyCollection<string> Words {
            get {
                if (words != null)
                    return words;

                char[] delimiters = { ' ', ',', '\t' };
                return words = new ReadOnlyCollection<string>(line.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries));
            }
        }
    }
}
