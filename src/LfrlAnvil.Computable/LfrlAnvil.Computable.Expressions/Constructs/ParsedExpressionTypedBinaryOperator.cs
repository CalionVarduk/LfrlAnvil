using System;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public abstract class ParsedExpressionTypedBinaryOperator : ParsedExpressionBinaryOperator
{
    protected ParsedExpressionTypedBinaryOperator(Type leftArgumentType, Type rightArgumentType)
    {
        LeftArgumentType = leftArgumentType;
        RightArgumentType = rightArgumentType;
    }

    public Type LeftArgumentType { get; }
    public Type RightArgumentType { get; }
}
