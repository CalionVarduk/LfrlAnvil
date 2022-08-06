using System;

namespace LfrlAnvil.Mathematical.Expressions.Exceptions;

public class MathExpressionArgumentBufferTooSmallException : ArgumentException
{
    public MathExpressionArgumentBufferTooSmallException(int actualLength, int expectedMinLength, string paramName)
        : base( Resources.ArgumentBufferIsTooSmall( actualLength, expectedMinLength ), paramName ) { }
}
