using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid number of arguments during delegate invocation.
/// </summary>
public class InvalidParsedExpressionArgumentCountException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidParsedExpressionArgumentCountException"/> instance.
    /// </summary>
    /// <param name="actual">Provided number of arguments.</param>
    /// <param name="expected">Expected number of arguments.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public InvalidParsedExpressionArgumentCountException(int actual, int expected, string paramName)
        : base( Resources.InvalidExpressionArgumentCount( actual, expected, paramName ), paramName )
    {
        Actual = actual;
        Expected = expected;
    }

    /// <summary>
    /// Provided number of arguments.
    /// </summary>
    public int Actual { get; }

    /// <summary>
    /// Expected number of arguments.
    /// </summary>
    public int Expected { get; }
}
