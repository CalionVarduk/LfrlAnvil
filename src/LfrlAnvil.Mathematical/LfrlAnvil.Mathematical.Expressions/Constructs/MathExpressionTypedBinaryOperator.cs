using System;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public abstract class MathExpressionTypedBinaryOperator : MathExpressionBinaryOperator
{
    protected MathExpressionTypedBinaryOperator(Type leftArgumentType, Type rightArgumentType)
    {
        LeftArgumentType = leftArgumentType;
        RightArgumentType = rightArgumentType;
    }

    public Type LeftArgumentType { get; }
    public Type RightArgumentType { get; }
}
