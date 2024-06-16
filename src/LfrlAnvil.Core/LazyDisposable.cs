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
using LfrlAnvil.Async;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

/// <summary>
/// Represents a generic lazy container for an optional disposable object.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public sealed class LazyDisposable<T> : IDisposable
    where T : IDisposable
{
    private InterlockedBoolean _canAssign;
    private InterlockedBoolean _isDisposed;

    /// <summary>
    /// Creates a new <see cref="LazyDisposable{T}"/> instance without an <see cref="Inner"/> object.
    /// </summary>
    public LazyDisposable()
    {
        Inner = default;
        _canAssign = new InterlockedBoolean( true );
        _isDisposed = new InterlockedBoolean( false );
    }

    /// <summary>
    /// Optional underlying disposable object.
    /// </summary>
    public T? Inner { get; private set; }

    /// <summary>
    /// Specifies whether or not an underlying <see cref="Inner"/> object can be assigned to this instance.
    /// </summary>
    public bool CanAssign => _canAssign.Value;

    /// <summary>
    /// Specifies whether or not this instance has already been disposed.
    /// </summary>
    public bool IsDisposed => _isDisposed.Value;

    /// <inheritdoc />
    /// <remarks>Disposes the underlying <see cref="Inner"/> object if it exists.</remarks>
    public void Dispose()
    {
        if ( ! _isDisposed.WriteTrue() )
            return;

        if ( ! _canAssign.Value )
        {
            Assume.IsNotNull( Inner );
            Inner.Dispose();
        }
    }

    /// <summary>
    /// Assigns an object to the underlying <see cref="Inner"/> object.
    /// </summary>
    /// <param name="inner">Object to assign.</param>
    /// <exception cref="InvalidOperationException">When <see cref="Inner"/> object has already been assigned.</exception>
    /// <remarks>
    /// Will dispose the <see cref="Inner"/> object immediately upon assignment if this instance has been already disposed.
    /// </remarks>
    public void Assign(T inner)
    {
        if ( ! _canAssign.WriteFalse() )
            throw new InvalidOperationException( ExceptionResources.LazyDisposableCannotAssign );

        Inner = inner;
        if ( _isDisposed.Value )
            Inner.Dispose();
    }
}
