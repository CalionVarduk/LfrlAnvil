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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased prepared asynchronous query reader expression.
/// </summary>
/// <param name="Dialect">SQL dialect that this expression is associated with.</param>
/// <param name="RowType">Row type.</param>
/// <param name="Expression">Underlying compilable expression.</param>
public readonly record struct SqlAsyncQueryReaderExpression(SqlDialect Dialect, Type RowType, ISqlAsyncQueryLambdaExpression Expression);

/// <summary>
/// Represents a generic prepared asynchronous query reader expression.
/// </summary>
/// <typeparam name="TRow">Row type.</typeparam>
public readonly struct SqlAsyncQueryReaderExpression<TRow>
    where TRow : notnull
{
    internal SqlAsyncQueryReaderExpression(SqlAsyncQueryReaderExpression @base)
    {
        Assume.Equals( @base.RowType, typeof( TRow ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<ISqlAsyncQueryLambdaExpression<TRow>>( @base.Expression );
    }

    /// <summary>
    /// SQL dialect that this expression is associated with.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Underlying compilable expression.
    /// </summary>
    public ISqlAsyncQueryLambdaExpression<TRow> Expression { get; }

    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>New <see cref="SqlAsyncQueryReader{TRow}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlAsyncQueryReader<TRow> Compile()
    {
        return new SqlAsyncQueryReader<TRow>( Dialect, Expression.Compile() );
    }
}
