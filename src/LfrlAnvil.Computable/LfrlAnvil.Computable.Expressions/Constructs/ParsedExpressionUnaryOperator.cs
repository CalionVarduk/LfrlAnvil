using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public abstract class ParsedExpressionUnaryOperator
{
    [Pure]
    internal Expression Process(Expression operand)
    {
        var result = CreateResult( operand );
        return result;
    }

    [Pure]
    protected virtual Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return null;
    }

    [Pure]
    protected abstract Expression CreateUnaryExpression(Expression operand);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression CreateResult(Expression operand)
    {
        if ( operand is ConstantExpression constant )
            return TryCreateFromConstant( constant ) ?? CreateUnaryExpression( operand );

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
