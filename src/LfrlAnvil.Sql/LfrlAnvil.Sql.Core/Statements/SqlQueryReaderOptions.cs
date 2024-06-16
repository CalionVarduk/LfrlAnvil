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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents <see cref="SqlQueryReader"/> options.
/// </summary>
/// <param name="InitialBufferCapacity">Specifies the initial capacity of read rows buffer.</param>
public readonly record struct SqlQueryReaderOptions(int? InitialBufferCapacity)
{
    /// <summary>
    /// Creates an initial rows buffer.
    /// </summary>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>New <see cref="List{T}"/> instance.</returns>
    /// <remarks>
    /// When <see cref="InitialBufferCapacity"/> is not null,
    /// then it will be used to set the initial <see cref="List{T}.Capacity"/> of the returned buffer.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public List<TRow> CreateList<TRow>()
    {
        return InitialBufferCapacity.HasValue
            ? new List<TRow>( capacity: InitialBufferCapacity.Value )
            : new List<TRow>();
    }
}
