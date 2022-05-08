using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Requests
{
    public class InvalidRequestTypeException : Exception
    {
        public InvalidRequestTypeException(Type requestType, Type expectedType)
            : base( GetMessage( requestType, expectedType ) )
        {
            RequestType = requestType;
            ExpectedType = expectedType;
        }

        public Type RequestType { get; }
        public Type ExpectedType { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static string GetMessage(Type requestType, Type expectedType)
        {
            var requestText = requestType.FullName;
            var expectedText = expectedType.FullName;
            return $"{requestText} is not a valid request type because it implements a request interface with {expectedText} type.";
        }
    }
}
