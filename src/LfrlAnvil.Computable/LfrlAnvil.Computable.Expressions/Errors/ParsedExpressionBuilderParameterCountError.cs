using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation due to an invalid number of parameters.
/// </summary>
public sealed class ParsedExpressionBuilderParameterCountError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderParameterCountError(
        ParsedExpressionBuilderErrorType type,
        StringSegment token,
        int actual,
        int expected)
        : base( type, token )
    {
        ActualParameterCount = actual;
        ExpectedParameterCount = expected;
    }

    /// <summary>
    /// Actual number of parameters.
    /// </summary>
    public int ActualParameterCount { get; }

    /// <summary>
    /// Expected number of parameters.
    /// </summary>
    public int ExpectedParameterCount { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderParameterCountError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, actual: {ActualParameterCount}, expected: {ExpectedParameterCount}";
    }
}
