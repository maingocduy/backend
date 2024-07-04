using System;
namespace WebApplication3.Helper.Data
{
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException()
        {
        }

        public ForbiddenAccessException(string message)
            : base(message)
        {
        }

        public ForbiddenAccessException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
