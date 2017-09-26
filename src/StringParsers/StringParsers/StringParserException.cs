using System;
using System.Runtime.Serialization;

namespace StringParsers
{
    [Serializable]
    public class StringParserException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public StringParserException()
        {
        }

        public StringParserException(string message)
            : base(message)
        {
        }

        public StringParserException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected StringParserException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}