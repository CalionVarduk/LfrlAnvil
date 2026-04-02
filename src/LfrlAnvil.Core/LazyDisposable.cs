// Copyright 2024-2026 Łukasz Furlepa
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
using System.Threading;
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
    private SpinLock _lock;
    private T? _inner;
    private bool _canAssign;
    private bool _isDisposed;

    /// <summary>
    /// Creates a new <see cref="LazyDisposable{T}"/> instance without an <see cref="Inner"/> object.
    /// </summary>
    public LazyDisposable()
    {
        _inner = default;
        _canAssign = true;
        _isDisposed = false;
    }

    /// <summary>
    /// Optional underlying disposable object.
    /// </summary>
    public T? Inner
    {
        get
        {
            using ( SpinLockEntry.Enter( ref _lock ) )
                return _inner;
        }
    }

    /// <summary>
    /// Specifies whether an underlying <see cref="Inner"/> object can be assigned to this instance.
    /// </summary>
    public bool CanAssign
    {
        get
        {
            using ( SpinLockEntry.Enter( ref _lock ) )
                return _canAssign;
        }
    }

    /// <summary>
    /// Specifies whether this instance has already been disposed.
    /// </summary>
    public bool IsDisposed
    {
        get
        {
            using ( SpinLockEntry.Enter( ref _lock ) )
                return _isDisposed;
        }
    }

    /// <inheritdoc />
    /// <remarks>Disposes the underlying <see cref="Inner"/> object if it exists.</remarks>
    public void Dispose()
    {
        T inner;
        using ( SpinLockEntry.Enter( ref _lock ) )
        {
            if ( _isDisposed )
                return;

            _isDisposed = true;
            if ( _canAssign )
                return;

            Assume.IsNotNull( _inner );
            inner = _inner;
        }

        inner.Dispose();
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
        var dispose = false;
        using ( SpinLockEntry.Enter( ref _lock ) )
        {
            if ( ! _canAssign )
                throw new InvalidOperationException( ExceptionResources.LazyDisposableCannotAssign );

            _canAssign = false;
            _inner = inner;
            if ( _isDisposed )
                dispose = true;
        }

        if ( dispose )
            inner.Dispose();
    }
}
