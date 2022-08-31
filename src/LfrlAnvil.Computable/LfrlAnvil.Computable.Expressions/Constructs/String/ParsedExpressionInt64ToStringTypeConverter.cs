﻿using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

public sealed class ParsedExpressionInt64ToStringTypeConverter : ParsedExpressionTypeConverter<string, long>
{
    private readonly MethodInfo _toString;
    private readonly ConstantExpression _formatProvider;

    public ParsedExpressionInt64ToStringTypeConverter(IFormatProvider? formatProvider = null)
    {
        _formatProvider = Expression.Constant( formatProvider, typeof( IFormatProvider ) );
        _toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( typeof( long ) );
    }

    [Pure]
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetSourceValue( operand, out var value )
            ? Expression.Constant( value.ToString( ReinterpretCast.To<IFormatProvider>( _formatProvider.Value ) ) )
            : null;
    }

    [Pure]
    protected override Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Call( operand, _toString, _formatProvider );
    }
}
