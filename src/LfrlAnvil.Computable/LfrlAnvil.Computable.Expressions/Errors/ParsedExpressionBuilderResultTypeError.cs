using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation due to an invalid result type.
/// </summary>
public sealed class ParsedExpressionBuilderResultTypeError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderResultTypeError(
        ParsedExpressionBuilderErrorType type,
        Type resultType,
        Type expectedType)
        : base( type )
    {
        ResultType = resultType;
        ExpectedType = expectedType;
    }

    /// <summary>
    /// Actual result type.
    /// </summary>
    public Type ResultType { get; }

    /// <summary>
    /// Expected result type.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderResultTypeError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, result type: {ResultType.GetDebugString()}, expected type: {ExpectedType.GetDebugString()}";
    }
}
