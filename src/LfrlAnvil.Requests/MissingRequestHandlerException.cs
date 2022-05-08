using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Requests
{
    public class MissingRequestHandlerException : Exception
    {
        public MissingRequestHandlerException(Type requestType)
            : base( GetMessage( requestType ) )
        {
            RequestType = requestType;
        }

        public Type RequestType { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static string GetMessage(Type requestType)
        {
            var requestText = requestType.FullName;
            return $"Handler is missing for a request of {requestText} type.";
        }
    }
}
