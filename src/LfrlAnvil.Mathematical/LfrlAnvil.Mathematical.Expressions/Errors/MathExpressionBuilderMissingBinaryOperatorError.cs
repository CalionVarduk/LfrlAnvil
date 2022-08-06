using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Errors;

public sealed class MathExpressionBuilderMissingBinaryOperatorError : MathExpressionBuilderError
{
    internal MathExpressionBuilderMissingBinaryOperatorError(
        MathExpressionBuilderErrorType type,
        StringSlice token,
        Type leftArgumentType,
        Type rightArgumentType)
        : base( type, token )
    {
        LeftArgumentType = leftArgumentType;
        RightArgumentType = rightArgumentType;
    }

    public Type LeftArgumentType { get; }
    public Type RightArgumentType { get; }

    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, left argument type {LeftArgumentType.FullName}, right argument type {RightArgumentType.FullName}";
    }
}
