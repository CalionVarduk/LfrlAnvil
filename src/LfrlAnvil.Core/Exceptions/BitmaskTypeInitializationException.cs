using System;

namespace LfrlAnvil.Exceptions;

/// <summary>
/// Represents an error that occurred during closed <see cref="Bitmask{T}"/> type initialization.
/// </summary>
public class BitmaskTypeInitializationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="BitmaskTypeInitializationException"/> instance.
    /// </summary>
    /// <param name="type"><see cref="Bitmask{T}"/> type parameter.</param>
    /// <param name="message">Exception's <see cref="Exception.Message"/>.</param>
    public BitmaskTypeInitializationException(Type type, string message)
        : base( message )
    {
        Type = type;
    }

    /// <summary>
    /// Creates a new <see cref="BitmaskTypeInitializationException"/> instance.
    /// </summary>
    /// <param name="type"><see cref="Bitmask{T}"/> type parameter.</param>
    /// <param name="message">Exception's <see cref="Exception.Message"/>.</param>
    /// <param name="innerException">Exception's <see cref="Exception.InnerException"/>.</param>
    public BitmaskTypeInitializationException(Type type, string message, Exception innerException)
        : base( message, innerException )
    {
        Type = type;
    }

    /// <summary>
    /// <see cref="Bitmask{T}"/> type parameter.
    /// </summary>
    public Type Type { get; }
}
