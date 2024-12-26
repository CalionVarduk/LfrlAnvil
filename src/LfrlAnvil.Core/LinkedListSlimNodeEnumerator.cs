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
/// Lightweight enumerator implementation for slim linked lists
/// that internally uses <see cref="LinkedListSlimNode{T}"/> instances to enumerate the list.
/// </summary>
public struct LinkedListSlimNodeEnumerator<T> : IEnumerator<T>
{
    private readonly LinkedListSlimNode<T>? _head;
    private LinkedListSlimNode<T>? _current;
    private LinkedListSlimNode<T>? _next;

    /// <summary>
    /// Creates a new <see cref="LinkedListSlimNodeEnumerator{T}"/> instance.
    /// </summary>
    /// <param name="head">Head <see cref="LinkedListSlimNode{T}"/> instance to start enumerating from.</param>
    public LinkedListSlimNodeEnumerator(LinkedListSlimNode<T>? head)
    {
        _head = head;
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
