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
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a source of <see cref="ISqlColumnBuilder"/> modifications.
/// </summary>
/// <param name="Column">Modified column.</param>
/// <param name="Source">Source of column modifications.</param>
/// <typeparam name="T">SQL column builder type.</typeparam>
public readonly record struct SqlColumnModificationSource<T>(T Column, T Source)
    where T : ISqlColumnBuilder
{
    /// <summary>
    /// Creates a new <see cref="SqlColumnModificationSource{T}"/> instance with the same <see cref="Column"/> and <see cref="Source"/>.
    /// </summary>
    /// <param name="column">Modified column.</param>
    /// <returns>New <see cref="SqlColumnModificationSource{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnModificationSource<T> Self(T column)
    {
        return new SqlColumnModificationSource<T>( column, column );
    }
}
