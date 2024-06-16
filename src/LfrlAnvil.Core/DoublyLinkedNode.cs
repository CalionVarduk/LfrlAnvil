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
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Represents a generic doubly-linked list node.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>See <see cref="DoublyLinkedNodeSequence{T}"/> for more information about a collection that uses these nodes.</remarks>
public sealed class DoublyLinkedNode<T>
{
    private T _value;

    /// <summary>
    /// Creates a new <see cref="DoublyLinkedNode{T}"/> instance without <see cref="Prev"/> or <see cref="Next"/>.
    /// </summary>
    /// <param name="value">Node's value.</param>
    public DoublyLinkedNode(T value)
    {
        _value = value;
    }

    /// <summary>
    /// Previous (predecessor) node.
    /// </summary>
    public DoublyLinkedNode<T>? Prev { get; private set; }

    /// <summary>
    /// Next (successor) node.
    /// </summary>
    public DoublyLinkedNode<T>? Next { get; private set; }

    /// <summary>
    /// Gets or sets this node's value.
    /// </summary>
    public T Value
    {
        get => _value;
        set => _value = value;
    }

    /// <summary>
    /// Gets a reference to this node's value.
    /// </summary>
    public ref T ValueRef => ref _value;

    /// <summary>
    /// Links this instance and the provided <paramref name="other"/> node together
    /// by setting <paramref name="other"/> as this node's <see cref="Prev"/> node.
    /// </summary>
    /// <param name="other">Node to set as this node's predecessor.</param>
    /// <exception cref="ArgumentException">
    /// When this instance already has a predecessor or <paramref name="other"/> already has a successor.
    /// </exception>
    public void LinkPrev(DoublyLinkedNode<T> other)
    {
        Ensure.IsNull( Prev );
        Ensure.IsNull( other.Next );
        Prev = other;
        other.Next = this;
    }

    /// <summary>
    /// Links this instance and the provided <paramref name="other"/> node together
    /// by setting <paramref name="other"/> as this node's <see cref="Next"/> node.
    /// </summary>
    /// <param name="other">Node to set as this node's successor.</param>
    /// <exception cref="ArgumentException">
    /// When this instance already has a successor or <paramref name="other"/> already has a predecessor.
    /// </exception>
    public void LinkNext(DoublyLinkedNode<T> other)
    {
        Ensure.IsNull( Next );
        Ensure.IsNull( other.Prev );
        Next = other;
        other.Prev = this;
    }

    /// <summary>
    /// Unlinks this instance from its predecessor. Does nothing when <see cref="Prev"/> is null.
    /// </summary>
    public void UnlinkPrev()
    {
        if ( Prev is null )
            return;

        Prev.Next = null;
        Prev = null;
    }

    /// <summary>
    /// Unlinks this instance from its successor. Does nothing when <see cref="Next"/> is null.
    /// </summary>
    public void UnlinkNext()
    {
        if ( Next is null )
            return;

        Next.Prev = null;
        Next = null;
    }

    /// <summary>
    /// Unlinks this instance from its predecessor and successor. Does nothing when <see cref="Prev"/> and <see cref="Next"/> are null.
    /// </summary>
    public void Remove()
    {
        if ( Prev is null )
        {
            if ( Next is not null )
            {
                Next.Prev = null;
                Next = null;
            }

            return;
        }

        if ( Next is null )
        {
            Prev.Next = null;
            Prev = null;
            return;
        }

        Prev.Next = Next;
        Next.Prev = Prev;
        Next = null;
        Prev = null;
    }
}
