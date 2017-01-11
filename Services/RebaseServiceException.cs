using System;
using System.Collections.Generic;
using System.Text;

namespace Rebase.Services
{
    public class RebaseServiceException : Exception
    {
        public RebaseServiceException() : base() { }

        public RebaseServiceException(string message) : base(message) { }

        public RebaseServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
