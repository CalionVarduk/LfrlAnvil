using System;

namespace LfrlAnvil.Requests.Exceptions
{
    public class InvalidRequestTypeException : InvalidCastException
    {
        public InvalidRequestTypeException(Type requestType, Type expectedType)
            : base( Resources.InvalidRequestType( requestType, expectedType ) )
        {
            RequestType = requestType;
            ExpectedType = expectedType;
        }

        public Type RequestType { get; }
        public Type ExpectedType { get; }
    }
}
