using System;
using System.Runtime.Serialization;

namespace FileManipulator {
    public class LineIsCommentException : System.ApplicationException {
        public LineIsCommentException() { }

        public LineIsCommentException(string message) : base(message) { }

        public LineIsCommentException(string message, Exception innerException) : base(message, innerException) { }
    }
}