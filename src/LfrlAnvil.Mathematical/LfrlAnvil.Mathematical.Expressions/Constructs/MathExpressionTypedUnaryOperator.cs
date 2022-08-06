using System;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public abstract class MathExpressionTypedUnaryOperator : MathExpressionUnaryOperator
{
    protected MathExpressionTypedUnaryOperator(Type argumentType)
    {
        ArgumentType = argumentType;
    }

    public Type ArgumentType { get; }
}
