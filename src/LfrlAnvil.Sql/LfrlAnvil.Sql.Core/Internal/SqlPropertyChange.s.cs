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

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Creates instances of <see cref="SqlPropertyChange{T}"/> type.
/// </summary>
public static class SqlPropertyChange
{
    /// <summary>
    /// Creates a new <see cref="SqlPropertyChange{T}"/> instance.
    /// </summary>
    /// <param name="newValue">New value to set.</param>
    /// <param name="state">Optional custom state. Equal to null by default.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlPropertyChange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPropertyChange<T> Create<T>(T newValue, object? state = null)
    {
        return new SqlPropertyChange<T>( isActive: true, newValue, state );
    }

    /// <summary>
    /// Creates a new <see cref="SqlPropertyChange{T}"/> instance marked as cancelled.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlPropertyChange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlPropertyChange<T> Cancel<T>()
    {
        return new SqlPropertyChange<T>( isActive: false, default!, state: null );
    }
}
