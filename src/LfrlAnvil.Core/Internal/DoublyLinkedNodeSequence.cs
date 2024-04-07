using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Internal;

public readonly struct DoublyLinkedNodeSequence<T>
{
    public static readonly DoublyLinkedNodeSequence<T> Empty = new DoublyLinkedNodeSequence<T>();

    private DoublyLinkedNodeSequence(DoublyLinkedNode<T>? head, DoublyLinkedNode<T>? tail)
    {
        Head = head;
        Tail = tail;
    }

    public readonly DoublyLinkedNode<T>? Head;
    public readonly DoublyLinkedNode<T>? Tail;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public DoublyLinkedNodeSequence<T> AddFirst(DoublyLinkedNode<T> node)
    {
        if ( Head is null )
            return new DoublyLinkedNodeSequence<T>( node, node );

        node.LinkNext( Head );
        return new DoublyLinkedNodeSequence<T>( node, Tail );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public DoublyLinkedNodeSequence<T> AddLast(DoublyLinkedNode<T> node)
    {
        if ( Tail is null )
            return new DoublyLinkedNodeSequence<T>( node, node );

        node.LinkPrev( Tail );
        return new DoublyLinkedNodeSequence<T>( Head, node );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public DoublyLinkedNodeSequence<T> Remove(DoublyLinkedNode<T> node)
    {
        var head = ReferenceEquals( node, Head ) ? node.Next : Head;
        var tail = ReferenceEquals( node, Tail ) ? node.Prev : Tail;
        node.Remove();
        return new DoublyLinkedNodeSequence<T>( head, tail );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( Head );
    }

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

        public T Current => _current!.Value;
        object? IEnumerator.Current => Current;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            if ( _next is null )
                return false;

            _current = _next;
            _next = _next.Next;
            return true;
        }

        public void Dispose() { }

        void IEnumerator.Reset()
        {
            _current = null;
            _next = _head;
        }
    }
}
