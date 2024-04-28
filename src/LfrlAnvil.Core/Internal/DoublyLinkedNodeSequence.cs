using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Internal;

/// <summary>
/// A lightweight list of doubly-linked nodes.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <remarks>Use with caution due to this list's lax integrity validation, compared to <see cref="LinkedList{T}"/>.</remarks>
public readonly struct DoublyLinkedNodeSequence<T>
{
    /// <summary>
    /// An empty list.
    /// </summary>
    public static readonly DoublyLinkedNodeSequence<T> Empty = new DoublyLinkedNodeSequence<T>();

    private DoublyLinkedNodeSequence(DoublyLinkedNode<T>? head, DoublyLinkedNode<T>? tail)
    {
        Head = head;
        Tail = tail;
    }

    /// <summary>
    /// The first node in the list, or null when list is empty.
    /// </summary>
    public readonly DoublyLinkedNode<T>? Head;

    /// <summary>
    /// The last node in the list, or null when list is empty.
    /// </summary>
    public readonly DoublyLinkedNode<T>? Tail;

    /// <summary>
    /// Creates a new <see cref="DoublyLinkedNodeSequence{T}"/> instance by adding a new node at the start of this linked list.
    /// </summary>
    /// <param name="node">Node to add.</param>
    /// <returns>New <see cref="DoublyLinkedNodeSequence{T}"/> with <paramref name="node"/> added as its <see cref="Head"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public DoublyLinkedNodeSequence<T> AddFirst(DoublyLinkedNode<T> node)
    {
        if ( Head is null )
            return new DoublyLinkedNodeSequence<T>( node, node );

        node.LinkNext( Head );
        return new DoublyLinkedNodeSequence<T>( node, Tail );
    }

    /// <summary>
    /// Creates a new <see cref="DoublyLinkedNodeSequence{T}"/> instance by adding a new node at the end of this linked list.
    /// </summary>
    /// <param name="node">Node to add.</param>
    /// <returns>New <see cref="DoublyLinkedNodeSequence{T}"/> with <paramref name="node"/> added as its <see cref="Tail"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public DoublyLinkedNodeSequence<T> AddLast(DoublyLinkedNode<T> node)
    {
        if ( Tail is null )
            return new DoublyLinkedNodeSequence<T>( node, node );

        node.LinkPrev( Tail );
        return new DoublyLinkedNodeSequence<T>( Head, node );
    }

    /// <summary>
    /// Creates a new <see cref="DoublyLinkedNodeSequence{T}"/> instance by removing the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Node to remove.</param>
    /// <returns>New <see cref="DoublyLinkedNodeSequence{T}"/> with removed <paramref name="node"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public DoublyLinkedNodeSequence<T> Remove(DoublyLinkedNode<T> node)
    {
        var head = ReferenceEquals( node, Head ) ? node.Next : Head;
        var tail = ReferenceEquals( node, Tail ) ? node.Prev : Tail;
        node.Remove();
        return new DoublyLinkedNodeSequence<T>( head, tail );
    }

    /// <summary>
    /// Unlinks all nodes in this <see cref="DoublyLinkedNodeSequence{T}"/> instance and returns <see cref="Empty"/> linked list.
    /// </summary>
    /// <returns><see cref="Empty"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public DoublyLinkedNodeSequence<T> Clear()
    {
        if ( Head is null )
            return this;

        Assume.IsNull( Head.Prev );
        var node = Head.Next;
        while ( node is not null )
        {
            node.UnlinkPrev();
            node = node.Next;
        }

        return Empty;
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this <see cref="DoublyLinkedNodeSequence{T}"/>.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( Head );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="DoublyLinkedNodeSequence{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly DoublyLinkedNode<T>? _head;
        private DoublyLinkedNode<T>? _current;
        private DoublyLinkedNode<T>? _next;

        internal Enumerator(DoublyLinkedNode<T>? head)
        {
            _head = head;
            _current = null;
            _next = head;
        }

        /// <inheritdoc />
        public T Current => _current!.Value;

        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            if ( _next is null )
                return false;

            _current = _next;
            _next = _next.Next;
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
}
