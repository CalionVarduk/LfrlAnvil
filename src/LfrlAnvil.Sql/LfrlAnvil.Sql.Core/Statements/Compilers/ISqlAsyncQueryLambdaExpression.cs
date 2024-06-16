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
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a type-erased asynchronous query lambda expression.
/// </summary>
public interface ISqlAsyncQueryLambdaExpression
{
    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>Compiled <see cref="Delegate"/>.</returns>
    [Pure]
    Delegate Compile();
}

/// <summary>
/// Represents a generic asynchronous query lambda expression.
/// </summary>
/// <typeparam name="TRow">Row type.</typeparam>
public interface ISqlAsyncQueryLambdaExpression<TRow> : ISqlAsyncQueryLambdaExpression
    where TRow : notnull
{
    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>Compiled <see cref="Delegate"/>.</returns>
    [Pure]
    new Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult<TRow>>> Compile();
}
