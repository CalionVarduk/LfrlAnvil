using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mathematical.Expressions.Tokens;

public abstract class MathExpressionUnaryOperator : MathExpressionOperator
{
    protected MathExpressionUnaryOperator(string symbol, MathExpressionUnaryOperatorNotation notation)
        : base( symbol )
    {
        Notation = notation;
    }

    public MathExpressionUnaryOperatorNotation Notation { get; }

    public sealed override void Process(Stack<Expression> operandStack)
    {
        if ( ! operandStack.TryPop( out var operand ) )
            throw new Exception(); // TODO

        var result = CreateResult( operand );
        operandStack.Push( result );
    }

    protected virtual Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return null;
    }

    protected abstract Expression CreateUnaryExpression(Expression operand);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression CreateResult(Expression operand)
    {
        if ( operand.NodeType == ExpressionType.Constant )
            return TryCreateFromConstant( (ConstantExpression)operand ) ?? CreateUnaryExpression( operand );

        return CreateUnaryExpression( operand );
    }
}
