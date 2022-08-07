using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class InvalidMathExpressionArgumentCountException : ArgumentException
{
    public InvalidMathExpressionArgumentCountException(int actual, int expected, string paramName)
        : base( Resources.InvalidExpressionArgumentCount( actual, expected, paramName ), paramName )
    {
        Actual = actual;
        Expected = expected;
    }

    public int Actual { get; }
    public int Expected { get; }
}
