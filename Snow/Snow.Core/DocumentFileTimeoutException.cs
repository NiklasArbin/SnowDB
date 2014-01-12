using System;

namespace Snow.Core
{
    public class DocumentFileTimeoutException : Exception
    {
        public DocumentFileTimeoutException(string message)
            : base(message)
        { }
    }
}