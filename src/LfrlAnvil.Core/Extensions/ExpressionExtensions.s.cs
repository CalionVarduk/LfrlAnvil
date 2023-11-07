using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Expressions;

namespace LfrlAnvil.Extensions;

public static class ExpressionExtensions
{
    [Pure]
    public static string GetMemberName<T, TMember>(this Expression<Func<T, TMember>> source)
    {
        var body = source.Body;

        Ensure.IsInstanceOfType<MemberExpression>( body );
        var memberExpr = ReinterpretCast.To<MemberExpression>( body );
        Ensure.True( memberExpr.Expression == source.Parameters[0] );

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

    [Pure]
    public static Expression ReplaceParametersByName(
        this Expression expression,
        IReadOnlyDictionary<string, Expression> parametersToReplace)
    {
        var replacer = new ExpressionParameterByNameReplacer( parametersToReplace );
        var result = replacer.Visit( expression );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression ReplaceParameter(this Expression expression, ParameterExpression parameterToReplace, Expression replacement)
    {
        return expression.ReplaceParameters( new[] { parameterToReplace }, new[] { replacement } );
    }

    [Pure]
    public static Expression ReplaceParameters(
        this Expression expression,
        ParameterExpression[] parametersToReplace,
        Expression[] replacements)
    {
        var replacer = new ExpressionParameterReplacer( parametersToReplace, replacements );
        var result = replacer.Visit( expression );
        return result;
    }
}
