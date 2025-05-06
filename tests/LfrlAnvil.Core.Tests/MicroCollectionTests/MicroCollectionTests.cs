using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.MicroCollectionTests;

public class MicroCollectionTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnEmptyCollection()
    {
        var sut = MicroCollection<string>.Create();
        Assertion.All( sut.Count.TestEquals( 0 ), TestSequence( sut, [ ] ) ).Go();
    }

    [Fact]
    public void Add_ShouldAddItemToEmptyCollection()
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "foo" );
        Assertion.All( sut.Count.TestEquals( 1 ), TestSequence( sut, [ "foo" ] ) ).Go();
    }

    [Fact]
    public void Add_ShouldAddItemsSequentiallyToEmptyCollection()
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Add( "x6" );

        Assertion.All( sut.Count.TestEquals( 6 ), TestSequence( sut, [ "x1", "x2", "x3", "x4", "x5", "x6" ] ) ).Go();
    }

    [Fact]
    public void IndexOf_ShouldReturnMinusOne_WhenCollectionIsEmpty()
    {
        var sut = MicroCollection<string?>.Create();
        var result = sut.IndexOf( null );
        result.TestEquals( -1 ).Go();
    }

    [Theory]
    [InlineData( "x0", -1 )]
    [InlineData( "x1", 0 )]
    [InlineData( "x2", 1 )]
    [InlineData( "x3", 2 )]
    [InlineData( "x4", 3 )]
    [InlineData( "x5", 4 )]
    [InlineData( "x6", -1 )]
    public void IndexOf_ShouldReturnCorrectIndex_WhenItemExists(string item, int expected)
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );

        var result = sut.IndexOf( item );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Remove_ShouldDoNothingAndReturnFalse_WhenCollectionIsEmpty()
    {
        var sut = MicroCollection<string>.Create();
        var result = sut.Remove( "foo" );
        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_ShouldRemoveLastItem()
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "foo" );

        var result = sut.Remove( "foo" );

        Assertion.All( result.TestTrue(), sut.Count.TestEquals( 0 ), TestSequence( sut, [ ] ) ).Go();
    }

    [Fact]
    public void Remove_ShouldRemoveFirstItem_WhenCollectionContainsTwoItems()
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );

        var result = sut.Remove( "x1" );

        Assertion.All( result.TestTrue(), sut.Count.TestEquals( 1 ), TestSequence( sut, [ "x2" ] ) ).Go();
    }

    [Fact]
    public void Remove_ShouldRemoveLastItem_WhenCollectionContainsTwoItems()
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );

        var result = sut.Remove( "x2" );

        Assertion.All( result.TestTrue(), sut.Count.TestEquals( 1 ), TestSequence( sut, [ "x1" ] ) ).Go();
    }

    [Theory]
    [InlineData( "x1", "x2", "x3", "x4" )]
    [InlineData( "x2", "x1", "x3", "x4" )]
    [InlineData( "x3", "x1", "x2", "x4" )]
    [InlineData( "x4", "x1", "x2", "x3" )]
    public void Remove_ShouldRemoveCorrectItem_WhenCollectionContainsMoreThanTwoItems(
        string item,
        string expected1,
        string expected2,
        string expected3)
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        var result = sut.Remove( item );

        Assertion.All( result.TestTrue(), sut.Count.TestEquals( 3 ), TestSequence( sut, [ expected1, expected2, expected3 ] ) ).Go();
    }

    [Fact]
    public void Remove_ShouldDoNothingAndReturnFalse_WhenItemDoesNotExist()
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        var result = sut.Remove( "foo" );

        Assertion.All( result.TestFalse(), sut.Count.TestEquals( 4 ), TestSequence( sut, [ "x1", "x2", "x3", "x4" ] ) ).Go();
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenCollectionIsEmpty()
    {
        var sut = MicroCollection<string>.Create();
        sut.Clear();
        Assertion.All( sut.Count.TestEquals( 0 ), TestSequence( sut, [ ] ) ).Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllElements_WhenCollectionIsNotEmpty()
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );
        sut.Add( "x5" );
        sut.Add( "x6" );

        sut.Clear();

        Assertion.All( sut.Count.TestEquals( 0 ), TestSequence( sut, [ ] ) ).Go();
    }

    [Theory]
    [InlineData( 0, "x1" )]
    [InlineData( 1, "x2" )]
    [InlineData( 2, "x3" )]
    [InlineData( 3, "x4" )]
    public void Indexer_Getter_ShouldReturnCorrectItem(int index, string expected)
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        var result = sut[index];

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 4 )]
    public void Indexer_Getter_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        var action = Lambda.Of( () => sut[index] );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0, "foo", "x2", "x3", "x4" )]
    [InlineData( 1, "x1", "foo", "x3", "x4" )]
    [InlineData( 2, "x1", "x2", "foo", "x4" )]
    [InlineData( 3, "x1", "x2", "x3", "foo" )]
    public void Indexer_Setter_ShouldChangeCorrectItem(int index, string expected1, string expected2, string expected3, string expected4)
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        sut[index] = "foo";

        TestSequence( sut, [ expected1, expected2, expected3, expected4 ] ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 4 )]
    public void Indexer_Setter_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = MicroCollection<string>.Create();
        sut.Add( "x1" );
        sut.Add( "x2" );
        sut.Add( "x3" );
        sut.Add( "x4" );

        var action = Lambda.Of( () => sut[index] = "foo" );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Pure]
    private static Assertion TestSequence<T>(MicroCollection<T> collection, T[] items)
    {
        var list = new List<T>();
        foreach ( var item in collection )
            list.Add( item );

        return list.TestSequence( items );
    }
}
