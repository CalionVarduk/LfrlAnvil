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

using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an <see cref="SqlParameterBinder"/> bound to a specific source of parameters.
/// </summary>
/// <param name="Binder">Underlying parameter binder.</param>
/// <param name="Source">Bound source of parameters.</param>
public readonly record struct SqlParameterBinderExecutor(SqlParameterBinder Binder, IEnumerable<SqlParameter>? Source)
{
    /// <summary>
    /// Binds <see cref="Source"/> parameters to the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Command to bind parameters to.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Execute(IDbCommand command)
    {
        Binder.Bind( command, Source );
    }
}

/// <summary>
/// Represents an <see cref="SqlParameterBinder{TSource}"/> bound to a specific source of parameters.
/// </summary>
/// <param name="Binder">Underlying parameter binder.</param>
/// <param name="Source">Bound source of parameters.</param>
/// <typeparam name="TSource">Parameter source type.</typeparam>
public readonly record struct SqlParameterBinderExecutor<TSource>(SqlParameterBinder<TSource> Binder, TSource? Source)
    where TSource : notnull
{
    /// <summary>
    /// Binds <see cref="Source"/> parameters to the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Command to bind parameters to.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Execute(IDbCommand command)
    {
        Binder.Bind( command, Source );
    }
}
