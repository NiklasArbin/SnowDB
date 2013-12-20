using System;

namespace Snow.Core
{
    public class DocumentNotFoundException : Exception
    {
        public DocumentNotFoundException(string message)
            : base(message)
        {

        }
    }
}