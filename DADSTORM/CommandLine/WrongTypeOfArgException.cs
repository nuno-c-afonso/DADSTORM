using System;
using System.Runtime.Serialization;

namespace CommandLine {
    public class WrongTypeOfArgException : ApplicationException {
        public WrongTypeOfArgException() {
        }

        public WrongTypeOfArgException(string message) : base(message) {
        }

        public WrongTypeOfArgException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}