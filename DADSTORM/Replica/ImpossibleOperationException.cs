using System;
using System.Runtime.Serialization;

namespace Replica {
    [Serializable]
    public class ImpossibleOperationException : Exception {
        public ImpossibleOperationException() {
        }

        public ImpossibleOperationException(string message) : base(message) {
        }

        public ImpossibleOperationException(string message, Exception innerException) : base(message, innerException) {
        }

        protected ImpossibleOperationException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}