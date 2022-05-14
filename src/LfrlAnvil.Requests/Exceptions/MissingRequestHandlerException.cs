using System;

namespace LfrlAnvil.Requests.Exceptions
{
    public class MissingRequestHandlerException : InvalidOperationException
    {
        public MissingRequestHandlerException(Type requestType)
            : base( Resources.MissingRequestHandler( requestType ) )
        {
            RequestType = requestType;
        }

        public Type RequestType { get; }
    }
}
