using System;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid dependency or implementor type registration attempt.
/// </summary>
public class InvalidTypeRegistrationException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidTypeRegistrationException"/> instance.
    /// </summary>
    /// <param name="type">Invalid type.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public InvalidTypeRegistrationException(Type type, string paramName)
        : base( Resources.InvalidTypeRegistration( type ), paramName )
    {
        Type = type;
    }

    /// <summary>
    /// Invalid type.
    /// </summary>
    public Type Type { get; }
}
