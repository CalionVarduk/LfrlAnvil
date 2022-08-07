using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

public sealed class MathExpressionBigIntToStringTypeConverter : MathExpressionTypeConverter<string, BigInteger>
{
    private readonly MethodInfo _toString;
    private readonly ConstantExpression _formatProvider;

    public MathExpressionBigIntToStringTypeConverter(IFormatProvider? formatProvider = null)
    {
        _formatProvider = Expression.Constant( formatProvider, typeof( IFormatProvider ) );
        _toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( typeof( BigInteger ) );
    }

    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetSourceValue( operand, out var value )
            ? Expression.Constant( value.ToString( (IFormatProvider?)_formatProvider.Value ) )
            : null;
    }

    protected override Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Call( operand, _toString, _formatProvider );
    }
}
