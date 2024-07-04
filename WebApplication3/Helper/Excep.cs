using System.Globalization;

namespace WebApplication3.Helper
{
    public class Excep : Exception
    {
        public Excep() : base() { }

        public Excep(string message) : base(message) { }

        public Excep(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}
