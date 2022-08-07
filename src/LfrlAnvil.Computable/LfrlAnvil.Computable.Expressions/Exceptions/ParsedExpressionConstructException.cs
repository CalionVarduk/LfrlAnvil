using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionConstructException : InvalidOperationException
{
    public ParsedExpressionConstructException(string message, Type constructType)
        : base( message )
    {
        ConstructType = constructType;
    }

    public Type ConstructType { get; }
}
