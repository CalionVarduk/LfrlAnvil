﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.String;

public sealed class MathExpressionDecimalToStringTypeConverter : MathExpressionTypeConverter<string, decimal>
{
    private readonly MethodInfo _toString;
    private readonly ConstantExpression _formatProvider;

    public MathExpressionDecimalToStringTypeConverter(IFormatProvider? formatProvider = null)
    {
        _formatProvider = Expression.Constant( formatProvider, typeof( IFormatProvider ) );
        _toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( typeof( decimal ) );
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
