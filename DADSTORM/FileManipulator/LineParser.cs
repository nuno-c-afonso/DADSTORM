using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FileManipulator {
    public class LineParser {
        private string line;
        private ReadOnlyCollection<string> words;

        public LineParser(string line) {
            if (line == null || line.Equals(""))
                throw new EmptyLineException();
            else if (line.Length > 0 && line[0] == '%')
                throw new LineIsCommentException();

            this.line = line;
        }

        public ReadOnlyCollection<string> Words {
            get {
                if (words != null)
                    return words;

                return words = new ReadOnlyCollection<string>(splitAll());
            }
        }

        // Attempt to merge the three methods
        private List<string> splitAll() {
            string str = "";
            int count = 0;
            int lineSize = line.Length;
            bool inside = false;
            List<string> splitted = new List<string>();

            for (int i = 0; i < lineSize; i++) {
                if (line[i] == '"') {
                    count++;
                    if (str.Length > 0) {
                        if (inside)
                            str += '"';
                        splitted.Add(str);
                    }

                    str = inside ? "" : "\"";
                    inside = !inside;
                }

                else {
                    if (!inside && (line[i] == ' ' || line[i] == ',' || line[i] == '\t')) {
                        if (str.Length > 0) {
                            splitted.Add(str);
                            str = "";
                        }
                    }
                    else
                        str += line[i];
                }
            }

            if (count % 2 != 0)
                throw new LineHasOddNumberQuotesException();

            if (str.Length != 0)
                splitted.Add(str);

            return splitted;
        }
    }
}
