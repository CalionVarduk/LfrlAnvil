using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Collections.Tests.HeapTests;

public abstract class GenericHeapTests<T> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEmptyHeap()
    {
        var sut = new Heap<T>();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestEquals( Comparer<T>.Default ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateEmptyHeap_WithExplicitComparer()
    {
        var comparer = Comparer<T>.Create( (a, b) => a!.GetHashCode().CompareTo( b!.GetHashCode() ) );
        var sut = new Heap<T>( comparer );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestEquals( comparer ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectHeapWithDistinctItems()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 20 );

        var sut = new Heap<T>( items );

        Assertion.All(
                sut.TestSetEqual( items ),
                sut.Comparer.TestEquals( Comparer<T>.Default ),
                AssertHeapInvariant( sut ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectHeapWithRepeatingItems()
    {
        var distinctItems = Fixture.CreateManyDistinct<T>( count: 5 );
        var items = distinctItems.SelectMany( i => new[] { i, i, i, i } ).ToList();

        var sut = new Heap<T>( items );

        Assertion.All(
                sut.TestSetEqual( items ),
                sut.Comparer.TestEquals( Comparer<T>.Default ),
                AssertHeapInvariant( sut ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectHeapWithItems_WithExplicitComparer()
    {
        var comparer = Comparer<T>.Create( (a, b) => a!.GetHashCode().CompareTo( b!.GetHashCode() ) );
        var items = Fixture.CreateManyDistinct<T>( count: 20 );

        var sut = new Heap<T>( items, comparer );

        Assertion.All(
                sut.TestSetEqual( items ),
                sut.Comparer.TestEquals( comparer ),
                AssertHeapInvariant( sut ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemToEmptyHeapOnTop()
    {
        var item = Fixture.Create<T>();

        var sut = new Heap<T>();

        sut.Add( item );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[0].TestEquals( item ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemOnTop_WhenNewItemIsLessThanExistingItem()
    {
        var (item, other) = Fixture.CreateManyDistinctSorted<T>( count: 2 );

        var sut = new Heap<T>( new[] { other } );

        sut.Add( item );

        sut.TestSequence( [ item, other ] ).Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemAsLeftChild_WhenNewItemIsGreaterThanSoleExistingItem()
    {
        var (other, item) = Fixture.CreateManyDistinctSorted<T>( count: 2 );

        var sut = new Heap<T>( new[] { other } );

        sut.Add( item );

        sut.TestSequence( [ other, item ] ).Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemAsRightChild_WhenNewItemIsGreaterThanExistingItem_AndLeftChildExists()
    {
        var (other, item, left) = Fixture.CreateManyDistinctSorted<T>( count: 3 );

        var sut = new Heap<T>( new[] { other, left } );

        sut.Add( item );

        sut.TestSequence( [ other, left, item ] ).Go();
    }

    [Fact]
    public void Add_ShouldSatisfyHeapInvariant()
    {
        var items = Fixture.CreateMany<T>( count: 20 ).ToList();

        var sut = new Heap<T>();

        foreach ( var item in items ) sut.Add( item );

        Assertion.All(
                sut.TestSetEqual( items ),
                AssertHeapInvariant( sut ) )
            .Go();
    }

    [Fact]
    public void Peek_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();
        var action = Lambda.Of( () => sut.Peek() );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void TryPeek_ShouldReturnFalse_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();

        var result = sut.TryPeek( out var peeked );

        Assertion.All(
                result.TestFalse(),
                peeked.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TryPeek_ShouldReturnTrueAndReturnTopItem()
    {
        var item = Fixture.Create<T>();

        var sut = new Heap<T>( new[] { item } );

        var result = sut.TryPeek( out var peeked );

        Assertion.All(
                result.TestTrue(),
                peeked.TestEquals( item ) )
            .Go();
    }

    [Fact]
    public void TryPeek_ShouldReturnCorrectResultAndNotModifyHeap()
    {
        var items = Fixture.CreateManyDistinctSorted<T>( count: 10 );
        var expected = items[0];

        var sut = new Heap<T>( items );
        var heapifiedItems = sut.ToList();

        var result = sut.TryPeek( out var peeked );

        Assertion.All(
                result.TestTrue(),
                peeked.TestEquals( expected ),
                sut.TestSequence( heapifiedItems ) )
            .Go();
    }

    [Fact]
    public void Extract_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();
        var action = Lambda.Of( () => sut.Extract() );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void TryExtract_ShouldReturnFalse_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();

        var result = sut.TryExtract( out var extracted );

        Assertion.All(
                result.TestFalse(),
                extracted.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TryExtract_ShouldReturnTrueAndRemoveTopItem()
    {
        var item = Fixture.Create<T>();

        var sut = new Heap<T>( new[] { item } );

        var result = sut.TryExtract( out var extracted );

        Assertion.All(
                result.TestTrue(),
                extracted.TestEquals( item ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void TryExtract_ShouldSatisfyHeapInvariant()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 20 );

        var sut = new Heap<T>( items );
        var expectedExtracted = sut[0];

        var result = sut.TryExtract( out var extracted );

        Assertion.All(
                result.TestTrue(),
                extracted.TestEquals( expectedExtracted ),
                sut.TestSetEqual( items.Where( i => ! i!.Equals( expectedExtracted ) ) ),
                AssertHeapInvariant( sut ) )
            .Go();
    }

    [Fact]
    public void Pop_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();
        var action = Lambda.Of( () => sut.Pop() );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void TryPop_ShouldReturnFalse_WhenHeapIsEmpty()
    {
        var sut = new Heap<T>();

        var result = sut.TryPop();

        result.TestFalse().Go();
    }

    [Fact]
    public void TryPop_ShouldReturnTrueAndRemoveTopItem()
    {
        var item = Fixture.Create<T>();

        var sut = new Heap<T>( new[] { item } );

        var result = sut.TryPop();

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void TryPop_ShouldSatisfyHeapInvariant()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 20 );

        var sut = new Heap<T>( items );
        var expectedExtracted = sut[0];

        var result = sut.TryPop();

        Assertion.All(
                result.TestTrue(),
                sut.TestSetEqual( items.Where( i => ! i!.Equals( expectedExtracted ) ) ),
                AssertHeapInvariant( sut ) )
            .Go();
    }

    [Fact]
    public void Replace_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
    {
        var item = Fixture.Create<T>();
        var sut = new Heap<T>();

        var action = Lambda.Of( () => sut.Replace( item ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void TryReplace_ShouldReturnFalse_WhenHeapIsEmpty()
    {
        var item = Fixture.Create<T>();
        var sut = new Heap<T>();

        var result = sut.TryReplace( item, out var replaced );

        Assertion.All(
                result.TestFalse(),
                replaced.TestEquals( default ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void TryReplace_ShouldReturnTrueAndReplaceTopItem()
    {
        var (oldItem, newItem) = Fixture.CreateManyDistinct<T>( count: 2 );

        var sut = new Heap<T>( new[] { oldItem } );

        var result = sut.TryReplace( newItem, out var replaced );

        Assertion.All(
                result.TestTrue(),
                replaced.TestEquals( oldItem ),
                sut.TestSequence( [ newItem ] ) )
            .Go();
    }

    [Fact]
    public void TryReplace_ShouldSatisfyHeapInvariant()
    {
        var allItems = Fixture.CreateManyDistinct<T>( count: 21 );

        var newItem = allItems[^1];
        var items = allItems.Take( 20 ).ToList();

        var sut = new Heap<T>( items );
        var expectedReplaced = sut[0];

        var result = sut.TryReplace( newItem, out var replaced );

        Assertion.All(
                result.TestTrue(),
                replaced.TestEquals( expectedReplaced ),
                sut.TestSetEqual( items.Where( i => ! i!.Equals( expectedReplaced ) ).Append( newItem ) ),
                AssertHeapInvariant( sut ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var items = Fixture.CreateMany<T>( count: 10 );

        var sut = new Heap<T>( items );

        sut.Clear();

        sut.Count.TestEquals( 0 ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void IndexerGet_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var items = Fixture.CreateMany<T>( count: 3 );

        var sut = new Heap<T>( items );

        var action = Lambda.Of( () => sut[index] );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void IndexerGet_ShouldReturnCorrectResult(int index)
    {
        var items = Fixture.CreateManyDistinctSorted<T>( count: 3 );

        var sut = new Heap<T>
        {
            items[0],
            items[1],
            items[2]
        };

        var result = sut[index];

        result.TestEquals( items[index] ).Go();
    }

    [Pure]
    private static Assertion AssertHeapInvariant(Heap<T> heap)
    {
        var comparer = heap.Comparer;
        var maxParentIndex = (heap.Count >> 1) - 1;

        var assertions = new List<Assertion>();
        for ( var parentIndex = 0; parentIndex <= maxParentIndex; ++parentIndex )
        {
            var parent = heap[parentIndex];

            var leftChildIndex = Heap.GetLeftChildIndex( parentIndex );
            var rightChildIndex = Heap.GetRightChildIndex( parentIndex );

            var leftChild = heap[leftChildIndex];
            var leftChildComparisonResult = comparer.Compare( parent, leftChild );

            assertions.Add(
                leftChildComparisonResult.TestLessThanOrEqualTo(
                    0,
                    $"parent[@{parentIndex}: '{parent}'] <=> left-child[@{leftChildIndex}: '{leftChild}']" ) );

            if ( rightChildIndex >= heap.Count )
                continue;

            var rightChild = heap[rightChildIndex];
            var rightChildComparisonResult = comparer.Compare( parent, rightChild );

            assertions.Add(
                rightChildComparisonResult.TestLessThanOrEqualTo(
                    0,
                    $"parent[@{parentIndex}: '{parent}'] <=> right-child[@{rightChildIndex}: '{rightChild}']" ) );
        }

        return Assertion.All( "MinHeapInvariant", assertions );
    }
}
