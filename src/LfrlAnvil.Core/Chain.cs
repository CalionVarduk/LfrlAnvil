using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

/// <summary>
/// A lightweight representation of a sequence of linked nodes with values.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct Chain<T> : IReadOnlyCollection<T>
{
    /// <summary>
    /// Represents an empty sequence.
    /// </summary>
    public static readonly Chain<T> Empty = new Chain<T>();

    private readonly HeadNode? _head;
    private readonly Node? _tail;

    /// <summary>
    /// Creates a new <see cref="Chain{T}"/> instance from a single value.
    /// </summary>
    /// <param name="value">Single value.</param>
    public Chain(T value)
    {
        _head = new HeadNode( value );
        _tail = _head;
        Count = 1;
    }

    /// <summary>
    /// Creates a new <see cref="Chain{T}"/> instance from a collection of values.
    /// </summary>
    /// <param name="values">Collection of values.</param>
    public Chain(IEnumerable<T> values)
    {
        using var enumerator = values.GetEnumerator();
        if ( ! enumerator.MoveNext() )
        {
            _tail = _head = null;
            Count = 0;
            return;
        }

        _head = new HeadNode( enumerator.Current );
        _tail = _head;
        Count = 1;

        while ( enumerator.MoveNext() )
        {
            var node = new Node( enumerator.Current );
            _tail.Next = node;
            _tail = node;
            ++Count;
        }
    }

    /// <summary>
    /// Creates a new <see cref="Chain{T}"/> instance from another <see cref="Chain{T}"/> instance.
    /// </summary>
    /// <param name="other"><see cref="Chain{T}"/> instance to copy.</param>
    public Chain(Chain<T> other)
    {
        if ( other._head is null )
        {
            _tail = _head = null;
            Count = 0;
            return;
        }

        _head = new HeadNode( other._head.Value );
        _tail = _head;
        Count = 1;

        var otherNode = other._head.Next;
        while ( Count < other.Count )
        {
            Assume.IsNotNull( otherNode );
            var node = new Node( otherNode.Value );
            _tail.Next = node;
            _tail = node;
            otherNode = otherNode.Next;
            ++Count;
        }
    }

    private Chain(HeadNode head, Node tail, int count)
    {
        _head = head;
        _tail = tail;
        Count = count;
    }

    /// <inheritdoc />
    public int Count { get; }

    /// <summary>
    /// Specifies whether or not new values can be appended to this sequence.
    /// <see cref="Chain{T}"/> is considered extendable when it hasn't been attached to the end of another sequence
    /// and no value has been added after its last node.
    /// </summary>
    public bool IsExtendable => _head is null || (! _head.HasPrev && _tail!.Next is null);

    /// <summary>
    /// Specifies whether or not this sequence has been attached to the end of another sequence.
    /// </summary>
    public bool IsAttached => _head is not null && _head.HasPrev;

    /// <summary>
    /// Adds the provided <paramref name="value"/> to the end of this sequence.
    /// </summary>
    /// <param name="value">Value to add.</param>
    /// <returns>New <see cref="Chain{T}"/> instance.</returns>
    /// <exception cref="InvalidOperationException">When this sequence is not extendable.</exception>
    [Pure]
    public Chain<T> Extend(T value)
    {
        if ( _head is null )
        {
            var headNode = new HeadNode( value );
            return new Chain<T>( headNode, headNode, count: 1 );
        }

        Assume.IsNotNull( _tail );

        if ( _tail.Next is not null )
            throw new InvalidOperationException( ExceptionResources.ChainHasAlreadyBeenExtended );

        if ( _head.HasPrev )
            throw new InvalidOperationException( ExceptionResources.ChainCannotBeExtendedBecauseItIsAttachedToAnotherChain );

        var node = new Node( value );
        _tail.Next = node;
        return new Chain<T>( _head, node, Count + 1 );
    }

    /// <summary>
    /// Adds the provided collection of <paramref name="values"/> to the end of this sequence.
    /// </summary>
    /// <param name="values">Collection of values to add.</param>
    /// <returns>New <see cref="Chain{T}"/> instance.</returns>
    /// <exception cref="InvalidOperationException">When this sequence is not extendable.</exception>
    [Pure]
    public Chain<T> Extend(IEnumerable<T> values)
    {
        using var enumerator = values.GetEnumerator();
        if ( ! enumerator.MoveNext() )
            return this;

        HeadNode head;
        Node tail;
        var count = Count + 1;

        if ( _head is null )
        {
            head = new HeadNode( enumerator.Current );
            tail = head;
        }
        else
        {
            Assume.IsNotNull( _tail );

            if ( _tail.Next is not null )
                throw new InvalidOperationException( ExceptionResources.ChainHasAlreadyBeenExtended );

            if ( _head.HasPrev )
                throw new InvalidOperationException( ExceptionResources.ChainCannotBeExtendedBecauseItIsAttachedToAnotherChain );

            var node = new Node( enumerator.Current );
            head = _head;
            _tail.Next = node;
            tail = node;
        }

        while ( enumerator.MoveNext() )
        {
            var node = new Node( enumerator.Current );
            tail.Next = node;
            tail = node;
            ++count;
        }

        return new Chain<T>( head, tail, count );
    }

    /// <summary>
    /// Attaches the provided <paramref name="other"/> to the end of this sequence.
    /// </summary>
    /// <param name="other">Sequence to attach.</param>
    /// <returns>New <see cref="Chain{T}"/> instance.</returns>
    /// <exception cref="InvalidOperationException">When this sequence is not extendable.</exception>
    /// <remarks>This method does not allocate memory.</remarks>
    [Pure]
    public Chain<T> Extend(Chain<T> other)
    {
        if ( _head is null )
            return other;

        if ( other._head is null )
            return this;

        Assume.IsNotNull( _tail );
        Assume.IsNotNull( other._tail );

        if ( _tail.Next is not null )
            throw new InvalidOperationException( ExceptionResources.ChainHasAlreadyBeenExtended );

        if ( _head.HasPrev )
            throw new InvalidOperationException( ExceptionResources.ChainCannotBeExtendedBecauseItIsAttachedToAnotherChain );

        _tail.Next = other._head;
        other._head.HasPrev = true;
        return new Chain<T>( _head, other._tail, Count + other.Count );
    }

    /// <summary>
    /// Returns an extendable version of this <see cref="Chain{T}"/> instance.
    /// </summary>
    /// <returns>This <see cref="Chain{T}"/> instance or its copy when it's not extendable.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Chain<T> ToExtendable()
    {
        return IsExtendable ? this : new Chain<T>( this );
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this sequence.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _head, Count );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="Chain{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private Node? _node;
        private Node? _next;
        private int _index;
        private readonly Node? _head;
        private readonly int _count;

        internal Enumerator(Node? node, int count)
        {
            _head = _node = _next = node;
            _count = count;
            _index = -1;
        }

        /// <inheritdoc />
        public T Current => _node!.Value;

        object IEnumerator.Current => Current!;

        /// <inheritdoc />
        public bool MoveNext()
        {
            if ( ++_index == _count )
                return false;

            _node = _next;
            _next = _next?.Next;
            return true;
        }

        /// <inheritdoc />
        public void Dispose() { }

        void IEnumerator.Reset()
        {
            _node = _next = _head;
            _index = -1;
        }
    }

    internal class Node
    {
        internal Node(T value)
        {
            Value = value;
            Next = null;
        }

        internal T Value { get; }
        internal Node? Next { get; set; }

        [Pure]
        public override string ToString()
        {
            return $"{nameof( Value )}({Value})";
        }
    }

    internal sealed class HeadNode : Node
    {
        internal HeadNode(T value)
            : base( value )
        {
            HasPrev = false;
        }

        internal bool HasPrev { get; set; }
    }

    [Pure]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
