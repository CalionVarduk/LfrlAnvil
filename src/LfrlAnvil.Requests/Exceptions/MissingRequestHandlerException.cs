using System;

namespace LfrlAnvil.Requests.Exceptions;

/// <summary>
/// Represents an error due to missing <see cref="IRequestHandler{TRequest,TResult}"/> factory.
/// </summary>
public class MissingRequestHandlerException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MissingRequestHandlerException"/> instance.
    /// </summary>
    /// <param name="requestType">Request type.</param>
    public MissingRequestHandlerException(Type requestType)
        : base( Resources.MissingRequestHandler( requestType ) )
    {
        RequestType = requestType;
    }

    /// <summary>
    /// Request type.
    /// </summary>
    public Type RequestType { get; }
}
