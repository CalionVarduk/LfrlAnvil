using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionInvalidArrayElementException : InvalidOperationException
{
    public ParsedExpressionInvalidArrayElementException(Type expectedType, Type actualType)
        : base( Resources.InvalidArrayElementType( expectedType, actualType ) )
    {
        ExpectedType = expectedType;
        ActualType = actualType;
    }

    public Type ExpectedType { get; }
    public Type ActualType { get; }
}
