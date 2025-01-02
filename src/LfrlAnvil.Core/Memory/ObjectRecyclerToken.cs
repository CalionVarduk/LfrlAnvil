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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Memory;

/// <summary>
/// A lightweight container for an underlying <see cref="ObjectRecycler{T}"/> object.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public readonly struct ObjectRecyclerToken<T> : IDisposable
    where T : class
{
    private readonly int _id;

    internal ObjectRecyclerToken(ObjectRecycler<T> owner, int id)
    {
        Owner = owner;
        _id = id;
    }

    /// <summary>
    /// <see cref="ObjectRecycler{T}"/> instance that owns this token.
    /// </summary>
    public ObjectRecycler<T>? Owner { get; }

    /// <inheritdoc/>
    /// <remarks>Frees the underlying object and returns it to the recycler for future use.</remarks>
    public void Dispose()
    {
        Owner?.Release( _id );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="ObjectRecyclerToken{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return ObjectRecycler<T>.GetString( Owner, _id );
    }

    /// <summary>
    /// Extracts the underlying object from this token.
    /// </summary>
    /// <returns>Underlying object.</returns>
    /// <exception cref="ObjectDisposedException">When this token has been disposed.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T GetObject()
    {
        if ( Owner is null )
            ObjectDisposedException.ThrowIf( Owner is null, typeof( ObjectRecyclerToken<T> ) );

        return Owner.GetObject( _id );
    }
}
