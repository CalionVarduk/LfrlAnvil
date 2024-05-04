using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to an argument buffer being too small.
/// </summary>
public class ParsedExpressionArgumentBufferTooSmallException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionArgumentBufferTooSmallException"/> instance.
    /// </summary>
    /// <param name="actualLength">Provided capacity.</param>
    /// <param name="expectedMinLength">Expected minimum number of elements.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public ParsedExpressionArgumentBufferTooSmallException(int actualLength, int expectedMinLength, string paramName)
        : base( Resources.ArgumentBufferIsTooSmall( actualLength, expectedMinLength ), paramName ) { }
}
