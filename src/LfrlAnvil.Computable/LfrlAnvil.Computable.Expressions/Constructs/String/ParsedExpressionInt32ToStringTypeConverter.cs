using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

public sealed class ParsedExpressionInt32ToStringTypeConverter : ParsedExpressionTypeConverter<string, int>
{
    private readonly MethodInfo _toString;
    private readonly ConstantExpression _formatProvider;

    public ParsedExpressionInt32ToStringTypeConverter(IFormatProvider? formatProvider = null)
    {
        _formatProvider = Expression.Constant( formatProvider, typeof( IFormatProvider ) );
        _toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( typeof( int ) );
    }

    [Pure]
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetSourceValue( operand, out var value )
            ? Expression.Constant( value.ToString( (IFormatProvider?)_formatProvider.Value ) )
            : null;
    }

    [Pure]
    protected override Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Call( operand, _toString, _formatProvider );
    }
}
