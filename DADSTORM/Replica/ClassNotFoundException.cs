using System;
using System.Runtime.Serialization;

namespace Replica {
    [Serializable]
    public class ClassNotFoundException : ApplicationException {
        public ClassNotFoundException() {
        }

        public ClassNotFoundException(string message) : base(message) {
        }

        public ClassNotFoundException(string message, Exception innerException) : base(message, innerException) {
        }

        protected ClassNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}