using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public abstract class ParsedExpressionBinaryOperator
{
    internal Expression Process(Expression leftOperand, Expression rightOperand)
    {
        var result = CreateResult( leftOperand, rightOperand );
        return result;
    }

    protected virtual Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return null;
    }

    protected virtual Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return null;
    }

    protected virtual Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return null;
    }

    protected abstract Expression CreateBinaryExpression(Expression left, Expression right);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression CreateResult(Expression left, Expression right)
    {
        if ( left.NodeType == ExpressionType.Constant )
        {
            if ( right.NodeType == ExpressionType.Constant )
            {
                return TryCreateFromTwoConstants( (ConstantExpression)left, (ConstantExpression)right ) ??
                    CreateBinaryExpression( left, right );
            }

            return TryCreateFromOneConstant( (ConstantExpression)left, right ) ?? CreateBinaryExpression( left, right );
        }

        if ( right.NodeType == ExpressionType.Constant )
            return TryCreateFromOneConstant( left, (ConstantExpression)right ) ?? CreateBinaryExpression( left, right );

        return CreateBinaryExpression( left, right );
    }
}

public abstract class ParsedExpressionBinaryOperator<TLeftArg, TRightArg> : ParsedExpressionTypedBinaryOperator
{
    protected ParsedExpressionBinaryOperator()
        : base( typeof( TLeftArg ), typeof( TRightArg ) ) { }

    protected static bool TryGetLeftArgumentValue(ConstantExpression expression, [MaybeNullWhen( false )] out TLeftArg result)
    {
        return expression.TryGetValue( out result );
    }

    protected static bool TryGetRightArgumentValue(ConstantExpression expression, [MaybeNullWhen( false )] out TRightArg result)
    {
        return expression.TryGetValue( out result );
    }
}

public abstract class ParsedExpressionBinaryOperator<TArg> : ParsedExpressionTypedBinaryOperator
{
    protected ParsedExpressionBinaryOperator()
        : base( typeof( TArg ), typeof( TArg ) ) { }

    protected static bool TryGetArgumentValue(ConstantExpression expression, [MaybeNullWhen( false )] out TArg result)
    {
        return expression.TryGetValue( out result );
    }
}
