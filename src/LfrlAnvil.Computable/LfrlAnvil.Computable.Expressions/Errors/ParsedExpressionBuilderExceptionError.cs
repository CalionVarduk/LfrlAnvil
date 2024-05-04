using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation due to an unexpected factory exception.
/// </summary>
public sealed class ParsedExpressionBuilderExceptionError : ParsedExpressionBuilderError
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBuilderExceptionError"/> instance.
    /// </summary>
    /// <param name="exception">Thrown exception.</param>
    public ParsedExpressionBuilderExceptionError(Exception exception)
    {
        Exception = exception;
    }

    internal ParsedExpressionBuilderExceptionError(Exception exception, ParsedExpressionBuilderErrorType type, StringSegment? token)
        : base( type, token )
    {
        Exception = exception;
    }

    /// <summary>
    /// Thrown exception.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderExceptionError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, an exception has been thrown:{Environment.NewLine}{Exception}";
    }
}
