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
/// Represents a type-erased prepared query reader expression.
/// </summary>
/// <param name="Dialect">SQL dialect that this expression is associated with.</param>
/// <param name="RowType">Row type.</param>
/// <param name="Expression">Underlying compilable expression.</param>
public readonly record struct SqlQueryReaderExpression(SqlDialect Dialect, Type RowType, LambdaExpression Expression);

/// <summary>
/// Represents a generic prepared query reader expression.
/// </summary>
/// <typeparam name="TRow">Row type.</typeparam>
public readonly struct SqlQueryReaderExpression<TRow>
    where TRow : notnull
{
    internal SqlQueryReaderExpression(SqlQueryReaderExpression @base)
    {
        Assume.Equals( @base.RowType, typeof( TRow ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<Expression<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<TRow>>>>( @base.Expression );
    }

    /// <summary>
    /// SQL dialect that this expression is associated with.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Underlying compilable expression.
    /// </summary>
    public Expression<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<TRow>>> Expression { get; }

    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>New <see cref="SqlQueryReader{TRow}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReader<TRow> Compile()
    {
        return new SqlQueryReader<TRow>( Dialect, Expression.Compile() );
    }
}
