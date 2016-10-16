using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FileManipulator {
    class LineParser {
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

                hasEvenNumberQuotes();

                List<string> splitted = splitByQuotes();

                splitted = finalSplit(splitted);

                return words = new ReadOnlyCollection<string>(splitted);
            }
        }

        private void hasEvenNumberQuotes() {
            int count = 0;

            foreach (char c in line)
                if (c == '"')
                    count++;

            if (count % 2 != 0)
                throw new LineHasOddNumberQuotesException();
        }

        private List<string> splitByQuotes() {
            string str = "";
            int lineSize = line.Length;
            bool inside = false;
            List<string> splitted = new List<string>();

            for (int i = 0; i < lineSize; i++) {
                if (line[i] == '"') {
                    if (i != 0) {
                        if (inside)
                            str += '"';
                        splitted.Add(str);
                    }

                    str = inside ? "" : "\"";
                    inside = !inside;
                }

                else
                    str += line[i];
            }

            if (str.Length != 0)
                splitted.Add(str);

            return splitted;
        }

        private List<string> finalSplit(List<string> toSplit) {
            char[] delimiters = { ' ', ',', '\t' };
            int listSize = toSplit.Count;
            List<string> res = new List<string>();

            for (int i = 0; i < listSize; i++) {
                if (toSplit[i].StartsWith("\"")) {
                    res.Add(toSplit[i]);
                }

                else {
                    res.AddRange(toSplit[i].Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries));
                }
            }

            return res;
        }
    }
}
