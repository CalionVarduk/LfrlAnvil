using System;

namespace LfrlAnvil.Mathematical.Expressions.Exceptions;

public class MathExpressionConstructException : InvalidOperationException
{
    public MathExpressionConstructException(string message, Type constructType)
        : base( message )
    {
        ConstructType = constructType;
    }

    public Type ConstructType { get; }
}
