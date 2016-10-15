using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManipulator {
    public class EOFException : System.ApplicationException {
        public EOFException() { }

        public EOFException(string message) : base(message) { }

        public EOFException(string message, System.Exception inner) : base(message, inner) { }
    }
}
