// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents an explicit SQL parameter configuration for <see cref="ISqlParameterBinderFactory"/>.
/// </summary>
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

    /// <summary>
    /// Source type's field or property name.
    /// </summary>
    public string? MemberName { get; }

    /// <summary>
    /// Name of the SQL parameter.
    /// </summary>
    public string? TargetParameterName { get; }

    /// <summary>
    /// Index of the positional SQL parameter.
    /// </summary>
    public int? ParameterIndex { get; }

    /// <summary>
    /// Specifies whether or not null source values will be completely ignored.
    /// Overrides the <see cref="SqlParameterBinderCreationOptions.IgnoreNullValues"/> option when not null.
    /// </summary>
    public bool? IsIgnoredWhenNull { get; }

    /// <summary>
    /// Custom source value selector expression.
    /// </summary>
    public LambdaExpression? CustomSelector { get; }

    /// <summary>
    /// Specifies whether or not the associated source type member should be completely ignored.
    /// </summary>
    [MemberNotNullWhen( false, nameof( TargetParameterName ) )]
    public bool IsIgnored => TargetParameterName is null;

    /// <summary>
    /// Source type from the <see cref="CustomSelector"/>.
    /// </summary>
    public Type? CustomSelectorSourceType => CustomSelector?.Parameters[0].Type;

    /// <summary>
    /// Value type from the <see cref="CustomSelector"/>.
    /// </summary>
    public Type? CustomSelectorValueType => CustomSelector?.Body.Type;

    /// <summary>
    /// Creates a new <see cref="SqlParameterConfiguration"/> instance that causes the provided source type member to be ignored.
    /// </summary>
    /// <param name="memberName">Source type's field or property name.</param>
    /// <returns>New <see cref="SqlParameterConfiguration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterConfiguration IgnoreMember(string memberName)
    {
        return new SqlParameterConfiguration( memberName, null, null, true, null );
    }

    /// <summary>
    /// Creates a new <see cref="SqlParameterConfiguration"/> instance that causes the provided source type member to
    /// potentially be ignored when its value is null.
    /// </summary>
    /// <param name="memberName">Source type's field or property name.</param>
    /// <param name="enabled">
    /// Specifies whether or not null source values will be completely ignored. Equal to <b>true</b> by default.
    /// </param>
    /// <param name="parameterIndex">Optional 0-based index to mark the parameter as positional. Equal to null by default.</param>
    /// <returns>New <see cref="SqlParameterConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="parameterIndex"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterConfiguration IgnoreMemberWhenNull(string memberName, bool enabled = true, int? parameterIndex = null)
    {
        return new SqlParameterConfiguration( memberName, memberName, parameterIndex, enabled, null );
    }

    /// <summary>
    /// Creates a new <see cref="SqlParameterConfiguration"/> instance that causes the provided source type member to
    /// be treated as a positional parameter.
    /// </summary>
    /// <param name="memberName">Source type's field or property name.</param>
    /// <param name="parameterIndex">Optional 0-based index to mark the parameter as positional.</param>
    /// <param name="isIgnoredWhenNull">
    /// Specifies whether or not null source values will be completely ignored. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlParameterConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="parameterIndex"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterConfiguration Positional(string memberName, int parameterIndex, bool? isIgnoredWhenNull = null)
    {
        return new SqlParameterConfiguration( memberName, memberName, parameterIndex, isIgnoredWhenNull, null );
    }

    /// <summary>
    /// Creates a new <see cref="SqlParameterConfiguration"/> instance that causes the provided source type member to
    /// create an SQL parameter with a different name.
    /// </summary>
    /// <param name="targetParameterName">Name of the SQL parameter.</param>
    /// <param name="memberName">Source type's field or property name.</param>
    /// <param name="isIgnoredWhenNull">
    /// Specifies whether or not null source values will be completely ignored. Equal to null by default.
    /// </param>
    /// <param name="parameterIndex">Optional 0-based index to mark the parameter as positional.</param>
    /// <returns>New <see cref="SqlParameterConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="parameterIndex"/> is less than <b>0</b>.</exception>
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

    /// <summary>
    /// Creates a new <see cref="SqlParameterConfiguration"/> instance that causes the provided SQL parameter to be created
    /// from a custom source value selector expression.
    /// </summary>
    /// <param name="targetParameterName">Name of the SQL parameter.</param>
    /// <param name="selector">Custom source value selector expression.</param>
    /// <param name="isIgnoredWhenNull">
    /// Specifies whether or not null source values will be completely ignored. Equal to null by default.
    /// </param>
    /// <param name="parameterIndex">Optional 0-based index to mark the parameter as positional.</param>
    /// <typeparam name="TSource">Parameter source type.</typeparam>
    /// <typeparam name="TValue">SQL parameter value type.</typeparam>
    /// <returns>New <see cref="SqlParameterConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="parameterIndex"/> is less than <b>0</b>.</exception>
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
