namespace WebApplication3.Helper.Data
{
    public class UnauthorizedAccessException : Exception
    {
        public UnauthorizedAccessException()
        {
        }

        public UnauthorizedAccessException(string message)
            : base(message)
        {
        }

        public UnauthorizedAccessException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
