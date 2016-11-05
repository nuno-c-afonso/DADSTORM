using System;
using System.Runtime.Serialization;

namespace Replica {
    [Serializable]
    internal class CouldNotInvokeMethodException : ApplicationException {
        public CouldNotInvokeMethodException() {
        }

        public CouldNotInvokeMethodException(string message) : base(message) {
        }

        public CouldNotInvokeMethodException(string message, Exception innerException) : base(message, innerException) {
        }

        protected CouldNotInvokeMethodException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}