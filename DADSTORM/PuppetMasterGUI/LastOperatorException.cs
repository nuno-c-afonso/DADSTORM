using System;
using System.Runtime.Serialization;

namespace PuppetMasterGUI {
    public class LastOperatorException : ApplicationException {
        public LastOperatorException() {
        }

        public LastOperatorException(string message) : base(message) {
        }

        public LastOperatorException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}