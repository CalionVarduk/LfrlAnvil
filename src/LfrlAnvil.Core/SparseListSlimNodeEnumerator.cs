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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

/// <summary>
/// Lightweight enumerator implementation for <see cref="SparseListSlim{T}"/>
/// that internally uses <see cref="SparseListSlim{T}.Node"/> instances to enumerate the list.
/// </summary>
public struct SparseListSlimNodeEnumerator<T> : IEnumerator<T>
{
    private readonly SparseListSlim<T>.Node? _head;
    private SparseListSlim<T>.Node? _current;
    private SparseListSlim<T>.Node? _next;

    /// <summary>
    /// Creates a new <see cref="SparseListSlimNodeEnumerator{T}"/> instance.
    /// </summary>
    /// <param name="items">Source <see cref="SparseListSlim{T}"/> instance to enumerate.</param>
    public SparseListSlimNodeEnumerator(SparseListSlim<T> items)
    {
        _head = items.First;
        _current = null;
        _next = _head;
    }

    /// <inheritdoc />
    public T Current => _current!.Value.Value;

    object? IEnumerator.Current => Current;

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool MoveNext()
    {
        if ( _next is null )
            return false;

        _current = _next;
        _next = _next.Value.Next;
        return true;
    }

    /// <inheritdoc />
    public void Dispose() { }

    void IEnumerator.Reset()
    {
        _current = null;
        _next = _head;
    }
}
