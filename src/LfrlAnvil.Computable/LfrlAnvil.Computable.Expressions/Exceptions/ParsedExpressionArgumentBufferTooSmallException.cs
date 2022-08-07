using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionArgumentBufferTooSmallException : ArgumentException
{
    public ParsedExpressionArgumentBufferTooSmallException(int actualLength, int expectedMinLength, string paramName)
        : base( Resources.ArgumentBufferIsTooSmall( actualLength, expectedMinLength ), paramName ) { }
}
