using System;
using System.Runtime.Serialization;

namespace CommandLine {
    public class IndexOutOfBoundsException : Exception {
        public IndexOutOfBoundsException() {
        }

        public IndexOutOfBoundsException(string message) : base(message) {
        }

        public IndexOutOfBoundsException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}