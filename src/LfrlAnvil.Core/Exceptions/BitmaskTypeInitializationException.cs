using System;

namespace LfrlAnvil.Exceptions;

public class BitmaskTypeInitializationException : InvalidOperationException
{
    public BitmaskTypeInitializationException(Type type, string message)
        : base( message )
    {
        Type = type;
    }

    public BitmaskTypeInitializationException(Type type, string message, Exception innerException)
        : base( message, innerException )
    {
        Type = type;
    }

    public Type Type { get; }
}
