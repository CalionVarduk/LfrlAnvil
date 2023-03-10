using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class InvalidTypeRegistrationException : ArgumentException
{
    public InvalidTypeRegistrationException(Type type, string paramName)
        : base( Resources.InvalidTypeRegistration( type ), paramName )
    {
        Type = type;
    }

    public Type Type { get; }
}
