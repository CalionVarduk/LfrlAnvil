﻿using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

public sealed class ParsedExpressionToStringTypeConverter : ParsedExpressionTypeConverter<string>
{
    private readonly MethodInfo _toString;

    public ParsedExpressionToStringTypeConverter()
    {
        _toString = MemberInfoLocator.FindToStringMethod();
    }

    [Pure]
    protected override Expression TryCreateFromConstant(ConstantExpression operand)
    {
        if ( operand.Value is null )
            throw new ArgumentNullException( nameof( operand ) + '.' + nameof( operand.Value ) );

        return Expression.Constant( operand.Value.ToString() );
    }

    [Pure]
    protected override Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Call( operand, _toString );
    }
}