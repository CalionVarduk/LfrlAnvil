using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements.Compilers;

public readonly struct SqlParameterConfiguration
{
    private SqlParameterConfiguration(
        string? memberName,
        string? targetParameterName,
        int? parameterIndex,
        bool? isIgnoredWhenNull,
        LambdaExpression? customSelector)
    {
        if ( parameterIndex is not null )
            Ensure.IsGreaterThanOrEqualTo( parameterIndex.Value, 0 );

        MemberName = memberName;
        TargetParameterName = targetParameterName;
        ParameterIndex = parameterIndex;
        IsIgnoredWhenNull = isIgnoredWhenNull;
        CustomSelector = customSelector;
    }

    public string? MemberName { get; }
    public string? TargetParameterName { get; }
    public int? ParameterIndex { get; }
    public bool? IsIgnoredWhenNull { get; }
    public LambdaExpression? CustomSelector { get; }

    [MemberNotNullWhen( false, nameof( TargetParameterName ) )]
    public bool IsIgnored => TargetParameterName is null;

    public Type? CustomSelectorSourceType => CustomSelector?.Parameters[0].Type;
    public Type? CustomSelectorValueType => CustomSelector?.Body.Type;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterConfiguration IgnoreMember(string memberName)
    {
        return new SqlParameterConfiguration( memberName, null, null, true, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterConfiguration IgnoreMemberWhenNull(string memberName, bool enabled = true, int? parameterIndex = null)
    {
        return new SqlParameterConfiguration( memberName, memberName, parameterIndex, enabled, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterConfiguration Positional(string memberName, int parameterIndex, bool? isIgnoredWhenNull = null)
    {
        return new SqlParameterConfiguration( memberName, memberName, parameterIndex, isIgnoredWhenNull, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterConfiguration From(
        string targetParameterName,
        string memberName,
        bool? isIgnoredWhenNull = null,
        int? parameterIndex = null)
    {
        return new SqlParameterConfiguration( memberName, targetParameterName, parameterIndex, isIgnoredWhenNull, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterConfiguration From<TSource, TValue>(
        string targetParameterName,
        Expression<Func<TSource, TValue>> selector,
        bool? isIgnoredWhenNull = null,
        int? parameterIndex = null)
    {
        return new SqlParameterConfiguration( null, targetParameterName, parameterIndex, isIgnoredWhenNull, selector );
    }
}
