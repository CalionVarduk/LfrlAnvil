﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.String;

public sealed class MathExpressionToStringTypeConverter : MathExpressionTypeConverter<string>
{
    private readonly MethodInfo _toString;

    public MathExpressionToStringTypeConverter()
    {
        _toString = MemberInfoLocator.FindToStringMethod();
    }

    protected override Expression TryCreateFromConstant(ConstantExpression operand)
    {
        if ( operand.Value is null )
            throw new ArgumentNullException( nameof( operand ) + '.' + nameof( operand.Value ) );

        return Expression.Constant( operand.Value.ToString() );
    }

    protected override Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Call( operand, _toString );
    }
}
