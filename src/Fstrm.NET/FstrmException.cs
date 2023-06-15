using System;

namespace Fstrm.NET
{
    public sealed class FstrmException : Exception
    {
        public FstrmException() : base()
        {
        }

        public FstrmException(string message) : base(message)
        {
        }

        public FstrmException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}