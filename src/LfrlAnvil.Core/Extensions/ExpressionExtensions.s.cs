﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

public static class ExpressionExtensions
{
    [Pure]
    public static string GetMemberName<T, TMember>(this Expression<Func<T, TMember>> source)
    {
        var body = source.Body;
        Ensure.True(
            body.NodeType == ExpressionType.MemberAccess,
            "Expression must be of the member access type." );

        var memberExpr = (MemberExpression)body;

        Ensure.True(
            memberExpr.Expression == source.Parameters[0],
            "Member expression's target must be the same as the expression's parameter." );

        return memberExpr.Member.Name;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryGetValue<T>(this ConstantExpression expression, [MaybeNullWhen( false )] out T result)
    {
        if ( expression.Value is T value )
        {
            result = value;
            return true;
        }

        result = default;
        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? GetValueOrDefault<T>(this ConstantExpression expression)
    {
        return expression.Value is T value ? value : default;
    }
}
