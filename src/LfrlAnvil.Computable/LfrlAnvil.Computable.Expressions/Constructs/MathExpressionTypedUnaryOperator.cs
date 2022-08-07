using System;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public abstract class MathExpressionTypedUnaryOperator : MathExpressionUnaryOperator
{
    protected MathExpressionTypedUnaryOperator(Type argumentType)
    {
        ArgumentType = argumentType;
    }

    public Type ArgumentType { get; }
}
