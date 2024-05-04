using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation due to a missing unary operator.
/// </summary>
public sealed class ParsedExpressionBuilderMissingUnaryOperatorError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingUnaryOperatorError(ParsedExpressionBuilderErrorType type, StringSegment token, Type argumentType)
        : base( type, token )
    {
        ArgumentType = argumentType;
    }

    /// <summary>
    /// Argument's type.
    /// </summary>
    public Type ArgumentType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderMissingUnaryOperatorError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, argument type: {ArgumentType.GetDebugString()}";
    }
}
