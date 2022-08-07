using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public abstract class ParsedExpressionUnaryOperator : IParsedExpressionConstruct
{
    public void Process(ParsedExpressionOperandStack operandStack)
    {
        Assume.IsNotEmpty( operandStack, nameof( operandStack ) );

        var operand = operandStack.Pop();
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

public abstract class ParsedExpressionUnaryOperator<TArg> : ParsedExpressionTypedUnaryOperator
{
    protected ParsedExpressionUnaryOperator()
        : base( typeof( TArg ) ) { }

    protected static bool TryGetArgumentValue(ConstantExpression expression, [MaybeNullWhen( false )] out TArg result)
    {
        return expression.TryGetValue( out result );
    }
}
