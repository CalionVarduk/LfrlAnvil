using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation
/// due to an exception thrown by a construct.
/// </summary>
public sealed class ParsedExpressionBuilderConstructError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderConstructError(
        ParsedExpressionBuilderErrorType type,
        object construct,
        StringSegment? token = null,
        Exception? exception = null)
        : base( type, token )
    {
        Construct = construct;
        Exception = exception;
    }

    /// <summary>
    /// Construct that has thrown the <see cref="Exception"/>.
    /// </summary>
    public object Construct { get; }

    /// <summary>
    /// Thrown exception.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderConstructError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var headerText = $"{base.ToString()}, construct of type {Construct.GetType().GetDebugString()}";
        if ( Exception is null )
            return headerText;

        return $"{headerText}, an exception has been thrown:{Environment.NewLine}{Exception}";
    }
}
