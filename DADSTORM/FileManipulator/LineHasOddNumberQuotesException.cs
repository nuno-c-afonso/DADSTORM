using System;

namespace FileManipulator {
    internal class LineHasOddNumberQuotesException : System.ApplicationException {
        public LineHasOddNumberQuotesException() { }

        public LineHasOddNumberQuotesException(string message) : base(message) { }

        public LineHasOddNumberQuotesException(string message, Exception innerException) : base(message, innerException) { }
    }
}