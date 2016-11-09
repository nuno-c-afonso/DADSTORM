using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses {
    public class UrlSpliter {
        public string getProtocol(string url) {
            char[] delimiters = { ':', '/' };
            string[] aux = url.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
            return aux[0];
        }

        public string getAddress(string url) {
            char[] delimiters = { ':', '/' };
            string[] aux = url.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
            return aux[1];
        }

        public string getPort(string url) {
            char[] delimiters = { ':', '/' };
            string[] aux = url.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);
            return aux[2];
        }
    }
}
