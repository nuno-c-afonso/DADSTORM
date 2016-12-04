using System;

namespace Replica {
    public class CouldNotSendTupleException : ApplicationException {
        public CouldNotSendTupleException() {
        }

        public CouldNotSendTupleException(string message) : base(message) {
        }

        public CouldNotSendTupleException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}