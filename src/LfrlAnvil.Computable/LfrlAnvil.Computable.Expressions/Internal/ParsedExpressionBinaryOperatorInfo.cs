using System;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

public readonly struct ParsedExpressionBinaryOperatorInfo
{
    internal ParsedExpressionBinaryOperatorInfo(Type operatorType, Type leftArgumentType, Type rightArgumentType)
    {
        OperatorType = operatorType;
        LeftArgumentType = leftArgumentType;
        RightArgumentType = rightArgumentType;
    }

    public Type OperatorType { get; }
    public Type LeftArgumentType { get; }
    public Type RightArgumentType { get; }

    public override string ToString()
    {
        return $"{OperatorType.GetDebugString()}({LeftArgumentType.GetDebugString()}, {RightArgumentType.GetDebugString()})";
    }
}
