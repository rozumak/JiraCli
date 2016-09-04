using System;

namespace JiraCli
{
    public class GracefulException : Exception
    {
        public GracefulException()
        {
        }

        public GracefulException(string message) : base(message)
        {
        }

        public GracefulException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}