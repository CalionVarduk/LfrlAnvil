using System;

namespace LfrlAnvil.Requests.Exceptions;

/// <summary>
/// Represents an error due to invalid <see cref="IRequest{TRequest,TResult}"/> type.
/// </summary>
public class InvalidRequestTypeException : InvalidCastException
{
    /// <summary>
    /// Creates a new <see cref="InvalidRequestTypeException"/> exception.
    /// </summary>
    /// <param name="requestType">Request type.</param>
    /// <param name="expectedType">Expected type.</param>
    public InvalidRequestTypeException(Type requestType, Type expectedType)
        : base( Resources.InvalidRequestType( requestType, expectedType ) )
    {
        RequestType = requestType;
        ExpectedType = expectedType;
    }

    /// <summary>
    /// Request type.
    /// </summary>
    public Type RequestType { get; }

    /// <summary>
    /// Expected type.
    /// </summary>
    public Type ExpectedType { get; }
}
