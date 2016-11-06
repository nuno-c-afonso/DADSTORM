using System;
using System.Runtime.Serialization;

namespace CommandLine {
    public class WrongNumberOfArgsException : ApplicationException {
        public WrongNumberOfArgsException() {
        }

        public WrongNumberOfArgsException(string message) : base(message) {
        }

        public WrongNumberOfArgsException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}