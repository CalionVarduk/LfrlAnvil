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
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased parameter binder.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
public readonly record struct SqlParameterBinder(SqlDialect Dialect, Action<IDbCommand, IEnumerable<SqlParameter>> Delegate)
{
    /// <summary>
    /// Binds the provided parameter collection to the <paramref name="command"/> or clears all of its <see cref="IDbCommand.Parameters"/>
    /// if no parameters have been specified.
    /// </summary>
    /// <param name="command">Command to bind parameters to.</param>
    /// <param name="source">Optional collection of parameters to bind.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Bind(IDbCommand command, IEnumerable<SqlParameter>? source = null)
    {
        if ( source is null )
            command.Parameters.Clear();
        else
            Delegate( command, source );
    }
}

/// <summary>
/// Represents a generic parameter binder.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
/// <typeparam name="TSource">Parameter source type.</typeparam>
public readonly record struct SqlParameterBinder<TSource>(SqlDialect Dialect, Action<IDbCommand, TSource> Delegate)
    where TSource : notnull
{
    /// <summary>
    /// Binds the provided parameter collection to the <paramref name="command"/> or clears all of its <see cref="IDbCommand.Parameters"/>
    /// if no parameters have been specified.
    /// </summary>
    /// <param name="command">Command to bind parameters to.</param>
    /// <param name="source">Optional source parameters to bind.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Bind(IDbCommand command, TSource? source = default)
    {
        if ( Generic<TSource>.IsNull( source ) )
            command.Parameters.Clear();
        else
            Delegate( command, source );
    }
}
