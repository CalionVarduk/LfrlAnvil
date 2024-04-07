using System.Collections.Generic;
using LfrlAnvil.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.DoublyLinkedNodeTests;

public class DoublyLinkedNodeSequenceTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnWithNullHeadAndTail()
    {
        var sut = DoublyLinkedNodeSequence<int>.Empty;
        using ( new AssertionScope() )
        {
            sut.Head.Should().BeNull();
            sut.Tail.Should().BeNull();
        }
    }

    [Fact]
    public void AddFirst_ToEmpty_ShouldReturnCorrectSequence()
    {
        var sut = DoublyLinkedNodeSequence<int>.Empty;
        var node = new DoublyLinkedNode<int>( 42 );

        var result = sut.AddFirst( node );

        using ( new AssertionScope() )
        {
            result.Head.Should().BeSameAs( node );
            result.Tail.Should().BeSameAs( node );
            node.Prev.Should().BeNull();
            node.Next.Should().BeNull();
        }
    }

    [Fact]
    public void AddFirst_ToNotEmpty_ShouldReturnCorrectSequence()
    {
        var head = new DoublyLinkedNode<int>( 123 );
        var sut = DoublyLinkedNodeSequence<int>.Empty.AddFirst( head );
        var node = new DoublyLinkedNode<int>( 42 );

        var result = sut.AddFirst( node );

        using ( new AssertionScope() )
        {
            result.Head.Should().BeSameAs( node );
            result.Tail.Should().BeSameAs( head );
            node.Prev.Should().BeNull();
            node.Next.Should().BeSameAs( head );
            head.Prev.Should().BeSameAs( node );
            head.Next.Should().BeNull();
        }
    }

    [Fact]
    public void AddLast_ToEmpty_ShouldReturnCorrectSequence()
    {
        var sut = DoublyLinkedNodeSequence<int>.Empty;
        var node = new DoublyLinkedNode<int>( 42 );

        var result = sut.AddLast( node );

        using ( new AssertionScope() )
        {
            result.Head.Should().BeSameAs( node );
            result.Tail.Should().BeSameAs( node );
            node.Prev.Should().BeNull();
            node.Next.Should().BeNull();
        }
    }

    [Fact]
    public void AddLast_ToNotEmpty_ShouldReturnCorrectSequence()
    {
        var tail = new DoublyLinkedNode<int>( 123 );
        var sut = DoublyLinkedNodeSequence<int>.Empty.AddLast( tail );
        var node = new DoublyLinkedNode<int>( 42 );

        var result = sut.AddLast( node );

        using ( new AssertionScope() )
        {
            result.Head.Should().BeSameAs( tail );
            result.Tail.Should().BeSameAs( node );
            node.Prev.Should().BeSameAs( tail );
            node.Next.Should().BeNull();
            tail.Prev.Should().BeNull();
            tail.Next.Should().BeSameAs( node );
        }
    }

    [Fact]
    public void Remove_WithOneNode_ShouldReturnEmpty()
    {
        var node = new DoublyLinkedNode<int>( 42 );
        var sut = DoublyLinkedNodeSequence<int>.Empty.AddFirst( node );

        var result = sut.Remove( node );

        using ( new AssertionScope() )
        {
            result.Head.Should().BeNull();
            result.Tail.Should().BeNull();
            node.Prev.Should().BeNull();
            node.Next.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_HeadWithMoreThanOneNode_ShouldReturnWithChangedHead()
    {
        var head = new DoublyLinkedNode<int>( 42 );
        var tail = new DoublyLinkedNode<int>( 123 );
        var sut = DoublyLinkedNodeSequence<int>.Empty.AddFirst( head ).AddLast( tail );

        var result = sut.Remove( head );

        using ( new AssertionScope() )
        {
            result.Head.Should().BeSameAs( tail );
            result.Tail.Should().BeSameAs( tail );
            head.Prev.Should().BeNull();
            head.Next.Should().BeNull();
            tail.Prev.Should().BeNull();
            tail.Next.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_TailWithMoreThanOneNode_ShouldReturnWithChangedTail()
    {
        var head = new DoublyLinkedNode<int>( 42 );
        var tail = new DoublyLinkedNode<int>( 123 );
        var sut = DoublyLinkedNodeSequence<int>.Empty.AddFirst( head ).AddLast( tail );

        var result = sut.Remove( tail );

        using ( new AssertionScope() )
        {
            result.Head.Should().BeSameAs( head );
            result.Tail.Should().BeSameAs( head );
            head.Prev.Should().BeNull();
            head.Next.Should().BeNull();
            tail.Prev.Should().BeNull();
            tail.Next.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_NotHeadAndNotTailWithMoreThanOneNode_ShouldReturnWithRemovedNode()
    {
        var head = new DoublyLinkedNode<int>( 42 );
        var node = new DoublyLinkedNode<int>( 84 );
        var tail = new DoublyLinkedNode<int>( 123 );
        var sut = DoublyLinkedNodeSequence<int>.Empty.AddFirst( head ).AddLast( node ).AddLast( tail );

        var result = sut.Remove( node );

        using ( new AssertionScope() )
        {
            result.Head.Should().BeSameAs( head );
            result.Tail.Should().BeSameAs( tail );
            head.Prev.Should().BeNull();
            head.Next.Should().BeSameAs( tail );
            tail.Prev.Should().BeSameAs( head );
            tail.Next.Should().BeNull();
            node.Prev.Should().BeNull();
            node.Next.Should().BeNull();
        }
    }

    [Fact]
    public void Clear_WithEmpty_ShouldReturnEmpty()
    {
        var sut = DoublyLinkedNodeSequence<int>.Empty;
        var result = sut.Clear();

        using ( new AssertionScope() )
        {
            result.Head.Should().BeNull();
            result.Tail.Should().BeNull();
        }
    }

    [Fact]
    public void Clear_WithNotEmpty_ShouldReturnEmpty()
    {
        var head = new DoublyLinkedNode<int>( 42 );
        var node = new DoublyLinkedNode<int>( 84 );
        var tail = new DoublyLinkedNode<int>( 123 );
        var sut = DoublyLinkedNodeSequence<int>.Empty.AddFirst( head ).AddLast( node ).AddLast( tail );

        var result = sut.Clear();

        using ( new AssertionScope() )
        {
            result.Head.Should().BeNull();
            result.Tail.Should().BeNull();
            head.Prev.Should().BeNull();
            head.Next.Should().BeNull();
            tail.Prev.Should().BeNull();
            tail.Next.Should().BeNull();
            node.Prev.Should().BeNull();
            node.Next.Should().BeNull();
        }
    }

    [Fact]
    public void GetEnumerator_WithEmpty_ShouldReturnCorrectResult()
    {
        var sut = DoublyLinkedNodeSequence<int>.Empty;

        var result = new List<int>();
        foreach ( var value in sut )
            result.Add( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_WithNotEmpty_ShouldReturnCorrectResult()
    {
        var head = new DoublyLinkedNode<int>( 42 );
        var node = new DoublyLinkedNode<int>( 84 );
        var tail = new DoublyLinkedNode<int>( 123 );
        var sut = DoublyLinkedNodeSequence<int>.Empty.AddFirst( head ).AddLast( node ).AddLast( tail );

        var result = new List<int>();
        foreach ( var value in sut )
            result.Add( value );

        result.Should().BeSequentiallyEqualTo( 42, 84, 123 );
    }
}
