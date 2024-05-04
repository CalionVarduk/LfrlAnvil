using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation due to a missing binary operator.
/// </summary>
public sealed class ParsedExpressionBuilderMissingBinaryOperatorError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingBinaryOperatorError(
        ParsedExpressionBuilderErrorType type,
        StringSegment token,
        Type leftArgumentType,
        Type rightArgumentType)
        : base( type, token )
    {
        LeftArgumentType = leftArgumentType;
        RightArgumentType = rightArgumentType;
    }

    /// <summary>
    /// Left argument's type.
    /// </summary>
    public Type LeftArgumentType { get; }

    /// <summary>
    /// Right argument's type.
    /// </summary>
    public Type RightArgumentType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderMissingBinaryOperatorError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"{base.ToString()}, left argument type: {LeftArgumentType.GetDebugString()}, right argument type: {RightArgumentType.GetDebugString()}";
    }
}
