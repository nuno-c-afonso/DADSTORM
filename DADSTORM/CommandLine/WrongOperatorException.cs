using System;
using System.Runtime.Serialization;

namespace CommandLine {
    public class WrongOperatorException : Exception {
        public WrongOperatorException() {
        }

        public WrongOperatorException(string message) : base(message) {
        }

        public WrongOperatorException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}