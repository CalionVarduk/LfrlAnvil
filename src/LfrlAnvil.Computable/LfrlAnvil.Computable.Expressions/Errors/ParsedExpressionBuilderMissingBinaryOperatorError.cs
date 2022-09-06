using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderMissingBinaryOperatorError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMissingBinaryOperatorError(
        ParsedExpressionBuilderErrorType type,
        StringSliceOld token,
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
        return
            $"{base.ToString()}, left argument type: {LeftArgumentType.GetDebugString()}, right argument type: {RightArgumentType.GetDebugString()}";
    }
}
