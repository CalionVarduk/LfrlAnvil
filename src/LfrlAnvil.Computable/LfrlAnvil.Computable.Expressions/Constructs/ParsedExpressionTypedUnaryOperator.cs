using System;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public abstract class ParsedExpressionTypedUnaryOperator : ParsedExpressionUnaryOperator
{
    protected ParsedExpressionTypedUnaryOperator(Type argumentType)
    {
        ArgumentType = argumentType;
    }

    public Type ArgumentType { get; }
}
