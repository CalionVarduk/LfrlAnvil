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
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased prepared parameter binder expression.
/// </summary>
/// <param name="Dialect">SQL dialect that this expression is associated with.</param>
/// <param name="SourceType">Parameter source type.</param>
/// <param name="Expression">Underlying compilable expression.</param>
public readonly record struct SqlParameterBinderExpression(SqlDialect Dialect, Type SourceType, LambdaExpression Expression);

/// <summary>
/// Represents a generic prepared parameter binder expression.
/// </summary>
/// <typeparam name="TSource">Parameter source type.</typeparam>
public readonly struct SqlParameterBinderExpression<TSource>
    where TSource : notnull
{
    internal SqlParameterBinderExpression(SqlParameterBinderExpression @base)
    {
        Assume.Equals( typeof( TSource ), @base.SourceType );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<Expression<Action<IDbCommand, TSource>>>( @base.Expression );
    }

    /// <summary>
    /// SQL dialect that this expression is associated with.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Underlying compilable expression.
    /// </summary>
    public Expression<Action<IDbCommand, TSource>> Expression { get; }

    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>New <see cref="SqlParameterBinder{TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlParameterBinder<TSource> Compile()
    {
        return new SqlParameterBinder<TSource>( Dialect, Expression.Compile() );
    }
}
