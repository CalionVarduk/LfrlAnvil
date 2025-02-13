using System.Collections;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Collections.Tests.RingTests;

public abstract class GenericRingTests<T> : TestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenNoParametersHaveBeenPassed()
    {
        var action = Lambda.Of( () => new Ring<T>() );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(int count)
    {
        var action = Lambda.Of( () => new Ring<T>( count ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void Ctor_ShouldCreateWithCorrectCount(int count)
    {
        var sut = new Ring<T>( count );

        Assertion.All(
                sut.Count.TestEquals( count ),
                sut.WriteIndex.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectItems()
    {
        var items = Fixture.CreateManyDistinct<T?>( count: 3 );
        var sut = new Ring<T>( items[0], items[1], items[2] );
        sut.TestSetEqual( items ).Go();
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 1 )]
    [InlineData( 2, 2 )]
    [InlineData( 3, 0 )]
    [InlineData( 4, 1 )]
    [InlineData( -1, 2 )]
    [InlineData( -2, 1 )]
    [InlineData( -3, 0 )]
    public void WriteIndexSet_ShouldUpdateWriteIndexCorrectly(int writeIndex, int expected)
    {
        var sut = new Ring<T>( count: 3 );

        sut.WriteIndex = writeIndex;
        var result = sut.WriteIndex;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void IndexerGet_ShouldThrowIndexOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var sut = new Ring<T>( count: 3 );
        var action = Lambda.Of( () => sut[index] );
        action.Test( exc => exc.TestType().Exact<IndexOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void IndexerSet_ShouldThrowIndexOutOfRangeException_WhenIndexIsOutOfBounds(int index)
    {
        var item = Fixture.Create<T>();

        var sut = new Ring<T>( count: 3 );

        var action = Lambda.Of( () => sut[index] = item );

        action.Test( exc => exc.TestType().Exact<IndexOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void IndexerSet_ShouldChangeCorrectItem(int index)
    {
        var item = Fixture.Create<T>();

        var sut = new Ring<T>( count: 3 );

        sut[index] = item;
        var result = sut[index];

        result.TestEquals( item ).Go();
    }

    [Theory]
    [InlineData( -3, 0 )]
    [InlineData( -2, 1 )]
    [InlineData( -1, 2 )]
    [InlineData( 0, 0 )]
    [InlineData( 1, 1 )]
    [InlineData( 2, 2 )]
    [InlineData( 3, 0 )]
    [InlineData( 4, 1 )]
    [InlineData( 5, 2 )]
    public void GetWrappedIndex_ShouldReturnCorrectResult(int index, int expected)
    {
        var sut = new Ring<T>( count: 3 );

        var result = sut.GetWrappedIndex( index );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0, 0, 0 )]
    [InlineData( 1, 0, 1 )]
    [InlineData( 2, 0, 2 )]
    [InlineData( 0, 1, 1 )]
    [InlineData( 1, 1, 2 )]
    [InlineData( 2, 1, 0 )]
    [InlineData( 0, 2, 2 )]
    [InlineData( 1, 2, 0 )]
    [InlineData( 2, 2, 1 )]
    [InlineData( 0, 3, 0 )]
    [InlineData( 1, 3, 1 )]
    [InlineData( 2, 3, 2 )]
    [InlineData( 0, 4, 1 )]
    [InlineData( 1, 4, 2 )]
    [InlineData( 2, 4, 0 )]
    [InlineData( 0, -1, 2 )]
    [InlineData( 1, -1, 0 )]
    [InlineData( 2, -1, 1 )]
    [InlineData( 0, -2, 1 )]
    [InlineData( 1, -2, 2 )]
    [InlineData( 2, -2, 0 )]
    [InlineData( 0, -3, 0 )]
    [InlineData( 1, -3, 1 )]
    [InlineData( 2, -3, 2 )]
    public void GetWriteIndex_ShouldReturnCorrectResult(int writeIndex, int offset, int expected)
    {
        var sut = new Ring<T>( count: 3 ) { WriteIndex = writeIndex };
        var result = sut.GetWriteIndex( offset );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 0 )]
    public void SetNext_ShouldChangeItemAtWriteIndexAndIncrementWriteIndex(int writeIndex, int expectedWriteIndex)
    {
        var item = Fixture.Create<T>();

        var sut = new Ring<T>( count: 3 ) { WriteIndex = writeIndex };

        sut.SetNext( item );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.WriteIndex.TestEquals( expectedWriteIndex ),
                sut[writeIndex].TestEquals( item ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void Clear_ShouldResetItemsAndWriteIndex(int writeIndex)
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );

        var sut = new Ring<T>( items ) { WriteIndex = writeIndex };

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.WriteIndex.TestEquals( 0 ),
                sut.TestAll( (e, _) => e.TestEquals( default ) ) )
            .Go();
    }

    [Theory]
    [InlineData( -3 )]
    [InlineData( -2 )]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    [InlineData( 6 )]
    public void Read_ShouldReturnCorrectResult(int readIndex)
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var expected = new[]
        {
            items[(0 + readIndex).EuclidModulo( 3 )], items[(1 + readIndex).EuclidModulo( 3 )], items[(2 + readIndex).EuclidModulo( 3 )]
        }.Select( x => ( T? )x );

        var sut = new Ring<T>( items );
        var result = sut.Read( readIndex );

        result.TestSequence( expected ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void GetEnumerator_ShouldReturnCorrectResult(int writeIndex)
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var expected
            = new[] { items[(0 + writeIndex) % 3], items[(1 + writeIndex) % 3], items[(2 + writeIndex) % 3] }.Select( x => ( T? )x );

        var sut = new Ring<T>( items ) { WriteIndex = writeIndex };

        sut.TestSequence( expected ).Go();
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 0 )]
    [InlineData( 2, 0 )]
    [InlineData( 0, 1 )]
    [InlineData( 1, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 0, 2 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 2 )]
    [InlineData( 0, 3 )]
    [InlineData( 1, 3 )]
    [InlineData( 2, 3 )]
    public void RingEnumeratorReset_ShouldResetEnumeratorCorrectly(int startIndex, int iterationCount)
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).ToArray();

        var ring = new Ring<T>( items ) { WriteIndex = startIndex };
        IEnumerator sut = ring.GetEnumerator();

        for ( var i = 0; i < iterationCount; ++i )
            sut.MoveNext();

        sut.Reset();
        sut.MoveNext();

        var firstItemAfterReset = sut.Current;

        var availableSteps = 1;
        while ( sut.MoveNext() )
            ++availableSteps;

        Assertion.All(
                firstItemAfterReset.TestNotNull( i => i.TestEquals( items[startIndex]! ) ),
                availableSteps.TestEquals( items.Length ) )
            .Go();
    }
}
