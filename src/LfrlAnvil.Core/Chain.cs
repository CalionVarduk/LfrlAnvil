using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

public readonly struct Chain<T> : IReadOnlyCollection<T>
{
    public static readonly Chain<T> Empty = new Chain<T>();

    private readonly HeadNode? _head;
    private readonly Node? _tail;

    public Chain(T value)
    {
        _head = new HeadNode( value );
        _tail = _head;
        Count = 1;
    }

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

    internal Chain(HeadNode head, Node tail, int count)
    {
        _head = head;
        _tail = tail;
        Count = count;
    }

    public int Count { get; }
    public bool IsExtendable => _head is null || (! _head.HasPrev && _tail!.Next is null);
    public bool IsAttached => _head is not null && _head.HasPrev;

    [Pure]
    public Chain<T> Extend(T value)
    {
        if ( _head is null )
        {
            var headNode = new HeadNode( value );
            return new Chain<T>( headNode, headNode, count: 1 );
        }

        Assume.IsNotNull( _tail, nameof( _tail ) );

        if ( _tail.Next is not null )
            throw new InvalidOperationException( ExceptionResources.ChainHasAlreadyBeenExtended );

        if ( _head.HasPrev )
            throw new InvalidOperationException( ExceptionResources.ChainCannotBeExtendedBecauseItIsAttachedToAnotherChain );

        var node = new Node( value );
        _tail.Next = node;
        return new Chain<T>( _head, node, Count + 1 );
    }

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
            Assume.IsNotNull( _tail, nameof( _tail ) );

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

    [Pure]
    public Chain<T> Extend(Chain<T> other)
    {
        if ( _head is null )
            return other;

        if ( other._head is null )
            return this;

        Assume.IsNotNull( _tail, nameof( _tail ) );
        Assume.IsNotNull( other._tail, nameof( other ) + '.' + nameof( _tail ) );

        if ( _tail.Next is not null )
            throw new InvalidOperationException( ExceptionResources.ChainHasAlreadyBeenExtended );

        if ( _head.HasPrev )
            throw new InvalidOperationException( ExceptionResources.ChainCannotBeExtendedBecauseItIsAttachedToAnotherChain );

        _tail.Next = other._head;
        other._head.HasPrev = true;
        return new Chain<T>( _head, other._tail, Count + other.Count );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _head, Count );
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

        public T Current => _node!.Value;
        object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            if ( ++_index == _count )
                return false;

            _node = _next;
            _next = _next?.Next;
            return true;
        }

        public void Dispose() { }

        public void Reset()
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
}
