using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Jasper {
    public class ParsingErrorException : Exception {
        public ParsingErrorException() : base() { }
        public ParsingErrorException(string message) : base(message) { }
    }
}
