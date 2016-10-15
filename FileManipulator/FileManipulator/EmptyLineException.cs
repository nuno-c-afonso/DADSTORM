using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FileManipulator {
    public class EmptyLineException : ApplicationException {
        public EmptyLineException() {
        }

        public EmptyLineException(string message) : base(message) {
        }

        public EmptyLineException(string message, Exception innerException) : base(message, innerException) {
        }

        protected EmptyLineException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
