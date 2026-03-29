// Copyright 2026 Łukasz Furlepa
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

using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an <see cref="SqlScalarQueryReaderExecutor"/> bound to an <see cref="SqlParameterBinder"/> instance.
/// </summary>
/// <param name="ParameterBinder">Underlying parameter binder.</param>
/// <param name="Reader">Underlying query reader.</param>
public readonly record struct SqlParameterizedScalarQueryReaderExecutor(
    SqlParameterBinder ParameterBinder,
    SqlScalarQueryReaderExecutor Reader
)
{
    /// <summary>
    /// Creates an <see cref="IDataReader"/> instance and reads a scalar value, using the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="command"><see cref="IDbCommand"/> to read from.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <returns>Returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult Execute(IDbCommand command, IEnumerable<SqlParameter>? parameters)
    {
        ParameterBinder.Bind( command, parameters );
        return Reader.Execute( command );
    }
}

/// <summary>
/// Represents an <see cref="SqlScalarQueryReaderExecutor{TRow}"/> bound to an <see cref="SqlParameterBinder{TSource}"/> instance.
/// </summary>
/// <param name="ParameterBinder">Underlying parameter binder.</param>
/// <param name="Reader">Underlying query reader.</param>
/// <typeparam name="TParameter">Parameter source type.</typeparam>
/// <typeparam name="T">Value type.</typeparam>
public readonly record struct SqlParameterizedScalarQueryReaderExecutor<TParameter, T>(
    SqlParameterBinder<TParameter> ParameterBinder,
    SqlScalarQueryReaderExecutor<T> Reader
)
    where TParameter : notnull
{
    /// <summary>
    /// Creates an <see cref="IDataReader"/> instance and reads a scalar value, using the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="command"><see cref="IDbCommand"/> to read from.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <returns>Returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult<T> Execute(IDbCommand command, TParameter? parameters)
    {
        ParameterBinder.Bind( command, parameters );
        return Reader.Execute( command );
    }
}
