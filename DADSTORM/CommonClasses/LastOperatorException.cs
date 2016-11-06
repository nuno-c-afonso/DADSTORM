using System;
using System.Runtime.Serialization;

namespace CommonClasses {
    public class LastOperatorException : ApplicationException {
        public LastOperatorException() {
        }

        public LastOperatorException(string message) : base(message) {
        }

        public LastOperatorException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}