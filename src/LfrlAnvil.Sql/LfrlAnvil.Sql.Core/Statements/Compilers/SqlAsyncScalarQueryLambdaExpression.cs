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
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a generic asynchronous scalar query lambda expression.
/// </summary>
/// <typeparam name="TDataReader">DB data reader type.</typeparam>
/// <typeparam name="T">Value type.</typeparam>
public sealed class SqlAsyncScalarQueryLambdaExpression<TDataReader, T> : ISqlAsyncScalarQueryLambdaExpression<T>
    where TDataReader : DbDataReader
{
    private SqlAsyncScalarQueryLambdaExpression(Expression<Func<TDataReader, SqlScalarQueryResult<T>>> readResultExpression)
    {
        ReadResultExpression = readResultExpression;
    }

    /// <summary>
    /// Underlying expression that reads and returns the scalar value.
    /// </summary>
    public Expression<Func<TDataReader, SqlScalarQueryResult<T>>> ReadResultExpression { get; }

    /// <summary>
    /// Creates a new <see cref="SqlAsyncScalarQueryLambdaExpression{TDataReader,T}"/> instance.
    /// </summary>
    /// <param name="readRowExpression">Underlying expression that reads and returns the scalar value.</param>
    /// <returns>New <see cref="SqlAsyncScalarQueryLambdaExpression{TDataReader,T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncScalarQueryLambdaExpression<TDataReader, T> Create(
        Expression<Func<TDataReader, SqlScalarQueryResult<T>>> readRowExpression)
    {
        return new SqlAsyncScalarQueryLambdaExpression<TDataReader, T>( readRowExpression );
    }

    /// <inheritdoc />
    [Pure]
    public Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult<T>>> Compile()
    {
        var readRowDelegate = ReadResultExpression.Compile();

        return async (reader, cancellationToken) =>
        {
            var concreteReader = ( TDataReader )reader;
            return await concreteReader.ReadAsync( cancellationToken ).ConfigureAwait( false )
                ? readRowDelegate( concreteReader )
                : SqlScalarQueryResult<T>.Empty;
        };
    }

    [Pure]
    Delegate ISqlAsyncScalarQueryLambdaExpression.Compile()
    {
        return Compile();
    }
}
