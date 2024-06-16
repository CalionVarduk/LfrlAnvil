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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an indexed SQL expression.
/// </summary>
/// <param name="Column">Optional <see cref="ISqlColumn"/> instance.</param>
/// <param name="Ordering">Ordering of this indexed expression.</param>
/// <typeparam name="T">SQL column type.</typeparam>
public readonly record struct SqlIndexed<T>(T? Column, OrderBy Ordering)
    where T : class, ISqlColumn
{
    /// <summary>
    /// Creates a new <see cref="SqlIndexed{T}"/> instance with base <see cref="ISqlColumn"/> type.
    /// </summary>
    /// <param name="source">Source to convert.</param>
    /// <returns>New <see cref="SqlIndexed{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlIndexed<ISqlColumn>(SqlIndexed<T> source)
    {
        return new SqlIndexed<ISqlColumn>( source.Column, source.Ordering );
    }
}
