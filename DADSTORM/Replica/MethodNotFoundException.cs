using System;
using System.Runtime.Serialization;

namespace Replica {
    [Serializable]
    public class MethodNotFoundException : ApplicationException {
        public MethodNotFoundException() {
        }

        public MethodNotFoundException(string message) : base(message) {
        }

        public MethodNotFoundException(string message, Exception innerException) : base(message, innerException) {
        }

        protected MethodNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}