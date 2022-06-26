using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Collections.Tests.HeapTests;

public abstract class GenericHeapTests<T> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEmptyHeap()
    {
        var sut = new Heap<T>();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().Be( Comparer<T>.Default );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateEmptyHeap_WithExplicitComparer()
    {
        var comparer = Comparer<T>.Create( (a, b) => a!.GetHashCode().CompareTo( b!.GetHashCode() ) );
        var sut = new Heap<T>( comparer );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().Be( comparer );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectHeapWithDistinctItems()
    {
        var items = Fixture.CreateDistinctCollection<T>( 20 );

        var sut = new Heap<T>( items );

        using ( new AssertionScope() )
        {
            sut.Should().BeEquivalentTo( items );
            sut.Comparer.Should().Be( Comparer<T>.Default );
            AssertHeapInvariant( sut );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectHeapWithRepeatingItems()
    {
        var distinctItems = Fixture.CreateDistinctCollection<T>( 5 );
        var items = distinctItems.SelectMany( i => new[] { i, i, i, i } ).ToList();

        var sut = new Heap<T>( items );

        using ( new AssertionScope() )
        {
            sut.Should().BeEquivalentTo( items );
            sut.Comparer.Should().Be( Comparer<T>.Default );
            AssertHeapInvariant( sut );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectHeapWithItems_WithExplicitComparer()
    {
        var comparer = Comparer<T>.Create( (a, b) => a!.GetHashCode().CompareTo( b!.GetHashCode() ) );
        var items = Fixture.CreateDistinctCollection<T>( 20 );

        var sut = new Heap<T>( items, comparer );

        using ( new AssertionScope() )
        {
            sut.Should().BeEquivalentTo( items );
            sut.Comparer.Should().Be( comparer );
            AssertHeapInvariant( sut );
        }
    }

    [Fact]
    public void Add_ShouldAddNewItemToEmptyHeapOnTop()
    {
        var item = Fixture.Create<T>();

        var sut = new Heap<T>();

        sut.Add( item );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[0].Should().Be( item );
        }
    }

    [Fact]
    public void Add_ShouldAddNewItemOnTop_WhenNewItemIsLessThanExistingItem()
    {
        var (item, other) = Fixture.CreateDistinctSortedCollection<T>( 2 );

        var sut = new Heap<T>( new[] { other } );

        sut.Add( item );

        sut.Should().BeSequentiallyEqualTo( item, other );
    }

    [Fact]
    public void Add_ShouldAddNewItemAsLeftChild_WhenNewItemIsGreaterThanSoleExistingItem()
    {
        var (other, item) = Fixture.CreateDistinctSortedCollection<T>( 2 );

        var sut = new Heap<T>( new[] { other } );

        sut.Add( item );

        sut.Should().BeSequentiallyEqualTo( other, item );
    }

    [Fact]
    public void Add_ShouldAddNewItemAsRightChild_WhenNewItemIsGreaterThanExistingItem_AndLeftChildExists()
    {
        var (other, item, left) = Fixture.CreateDistinctSortedCollection<T>( 3 );

        var sut = new Heap<T>( new[] { other, left } );

        sut.Add( item );

        sut.Should().BeSequentiallyEqualTo( other, left, item );
    }

    [Fact]
    public void Add_ShouldSatisfyHeapInvariant()
    {
        var items = Fixture.CreateMany<T>( 20 ).ToList();

        var sut = new Heap<T>();

        foreach ( var item in items )
            sut.Add( item );

        using ( new AssertionScope() )
        {
            sut.Should().BeEquivalentTo( items );
            AssertHeapInvariant( sut );
        }
    }

    [Fact]
    public void Peek_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();
        var action = Lambda.Of( () => sut.Peek() );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TryPeek_ShouldReturnFalse_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();

        var result = sut.TryPeek( out var peeked );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            peeked.Should().Be( default( T ) );
        }
    }

    [Fact]
    public void TryPeek_ShouldReturnTrueAndReturnTopItem()
    {
        var item = Fixture.Create<T>();

        var sut = new Heap<T>( new[] { item } );

        var result = sut.TryPeek( out var peeked );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            peeked.Should().Be( item );
        }
    }

    [Fact]
    public void TryPeek_ShouldReturnCorrectResultAndNotModifyHeap()
    {
        var items = Fixture.CreateDistinctSortedCollection<T>( 10 );
        var expected = items[0];

        var sut = new Heap<T>( items );
        var heapifiedItems = sut.ToList();

        var result = sut.TryPeek( out var peeked );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            peeked.Should().Be( expected );
            sut.Should().BeSequentiallyEqualTo( heapifiedItems );
        }
    }

    [Fact]
    public void Extract_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();
        var action = Lambda.Of( () => sut.Extract() );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TryExtract_ShouldReturnFalse_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();

        var result = sut.TryExtract( out var extracted );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            extracted.Should().Be( default( T ) );
        }
    }

    [Fact]
    public void TryExtract_ShouldReturnTrueAndRemoveTopItem()
    {
        var item = Fixture.Create<T>();

        var sut = new Heap<T>( new[] { item } );

        var result = sut.TryExtract( out var extracted );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            extracted.Should().Be( item );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void TryExtract_ShouldSatisfyHeapInvariant()
    {
        var items = Fixture.CreateDistinctCollection<T>( 20 );

        var sut = new Heap<T>( items );
        var expectedExtracted = sut[0];

        var result = sut.TryExtract( out var extracted );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            extracted.Should().Be( expectedExtracted );
            sut.Should().NotContain( expectedExtracted );
            sut.Should().BeEquivalentTo( items.Where( i => ! i!.Equals( expectedExtracted ) ) );
            AssertHeapInvariant( sut );
        }
    }

    [Fact]
    public void Pop_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();
        var action = Lambda.Of( () => sut.Pop() );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TryPop_ShouldReturnFalse_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();

        var result = sut.TryPop();

        result.Should().BeFalse();
    }

    [Fact]
    public void TryPop_ShouldReturnTrueAndRemoveTopItem()
    {
        var item = Fixture.Create<T>();

        var sut = new Heap<T>( new[] { item } );

        var result = sut.TryPop();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void TryPop_ShouldSatisfyHeapInvariant()
    {
        var items = Fixture.CreateDistinctCollection<T>( 20 );

        var sut = new Heap<T>( items );
        var expectedExtracted = sut[0];

        var result = sut.TryPop();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Should().NotContain( expectedExtracted );
            sut.Should().BeEquivalentTo( items.Where( i => ! i!.Equals( expectedExtracted ) ) );
            AssertHeapInvariant( sut );
        }
    }

    [Fact]
    public void Replace_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
    {
        var item = Fixture.Create<T>();
        var sut = new Heap<T>();

        var action = Lambda.Of( () => sut.Replace( item ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TryReplace_ShouldReturnFalse_WhenHeapIsEmpty()
    {
        var item = Fixture.Create<T>();
        var sut = new Heap<T>();

        var result = sut.TryReplace( item, out var replaced );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            replaced.Should().Be( default( T ) );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void TryReplace_ShouldReturnTrueAndReplaceTopItem()
    {
        var (oldItem, newItem) = Fixture.CreateDistinctCollection<T>( 2 );

        var sut = new Heap<T>( new[] { oldItem } );

        var result = sut.TryReplace( newItem, out var replaced );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            replaced.Should().Be( oldItem );
            sut.Should().BeSequentiallyEqualTo( newItem );
        }
    }

    [Fact]
    public void TryReplace_ShouldSatisfyHeapInvariant()
    {
        var allItems = Fixture.CreateDistinctCollection<T>( 21 );

        var newItem = allItems[^1];
        var items = allItems.Take( 20 ).ToList();

        var sut = new Heap<T>( items );
        var expectedReplaced = sut[0];

        var result = sut.TryReplace( newItem, out var replaced );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            replaced.Should().Be( expectedReplaced );
            sut.Should().NotContain( expectedReplaced );
            sut.Should().BeEquivalentTo( items.Where( i => ! i!.Equals( expectedReplaced ) ).Append( newItem ) );
            AssertHeapInvariant( sut );
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var items = Fixture.CreateMany<T>( 10 );

        var sut = new Heap<T>( items );

        sut.Clear();

        sut.Count.Should().Be( 0 );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void IndexerGet_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var items = Fixture.CreateMany<T>( 3 );

        var sut = new Heap<T>( items );

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void IndexerGet_ShouldReturnCorrectResult(int index)
    {
        var items = Fixture.CreateDistinctSortedCollection<T>( 3 );

        var sut = new Heap<T> { items[0], items[1], items[2] };

        var result = sut[index];

        result.Should().Be( items[index] );
    }

    private static void AssertHeapInvariant(Heap<T> heap)
    {
        var comparer = heap.Comparer;
        var maxParentIndex = (heap.Count >> 1) - 1;

        for ( var parentIndex = 0; parentIndex <= maxParentIndex; ++parentIndex )
        {
            var parent = heap[parentIndex];

            var leftChildIndex = Heap.GetLeftChildIndex( parentIndex );
            var rightChildIndex = Heap.GetRightChildIndex( parentIndex );

            var leftChild = heap[leftChildIndex];
            var leftChildComparisonResult = comparer.Compare( parent, leftChild );

            leftChildComparisonResult.Should()
                .BeLessOrEqualTo(
                    0,
                    "min heap invariant must be satisfied, which means that parent {0} must be less than or equal to its left child {1}",
                    parent,
                    leftChild );

            if ( rightChildIndex >= heap.Count )
                continue;

            var rightChild = heap[rightChildIndex];
            var rightChildComparisonResult = comparer.Compare( parent, rightChild );

            rightChildComparisonResult.Should()
                .BeLessOrEqualTo(
                    0,
                    "min heap invariant must be satisfied, which means that parent {0} must be less than or equal to its right child {1}",
                    parent,
                    rightChild );
        }
    }
}
