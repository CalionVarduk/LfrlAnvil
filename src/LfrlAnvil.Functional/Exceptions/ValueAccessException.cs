using System;

namespace LfrlAnvil.Functional.Exceptions
{
    public class ValueAccessException : InvalidOperationException
    {
        public ValueAccessException(string message, string memberName)
            : base( message )
        {
            MemberName = memberName;
        }

        public string MemberName { get; }
    }
}
