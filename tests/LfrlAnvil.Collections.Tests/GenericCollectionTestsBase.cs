using System.Collections.Generic;

namespace LfrlAnvil.Collections.Tests;

public abstract class GenericCollectionTestsBase<T> : TestsBase
{
    [Fact]
    public void ICollectionAdd_ShouldIncreaseCount()
    {
        var item = Fixture.Create<T>();
        var sut = CreateEmptyCollection();

        sut.Add( item );

        sut.Count.Should().Be( 1 );
    }

    [Fact]
    public void ICollectionRemove_ShouldReturnFalse_WhenCollectionIsEmpty()
    {
        var item = Fixture.Create<T>();
        var sut = CreateEmptyCollection();

        var result = sut.Remove( item );

        result.Should().BeFalse();
    }

    [Fact]
    public void ICollectionRemove_ShouldReturnTrueAndReduceCount_WhenItemExists()
    {
        var item = Fixture.Create<T>();
        var sut = CreateEmptyCollection();
        sut.Add( item );

        var result = sut.Remove( item );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void ICollectionContains_ShouldReturnTrue_WhenItemExists()
    {
        var item = Fixture.Create<T>();
        var sut = CreateEmptyCollection();
        sut.Add( item );

        var result = sut.Contains( item );

        result.Should().BeTrue();
    }

    [Fact]
    public void ICollectionContains_ShouldReturnFalse_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();
        var sut = CreateEmptyCollection();

        var result = sut.Contains( item );

        result.Should().BeFalse();
    }

    [Fact]
    public void ICollectionCopyTo_ShouldCopyItemsCorrectly()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = CreateEmptyCollection();
        sut.Add( items[0] );
        sut.Add( items[1] );
        sut.Add( items[2] );

        var array = new T[3];
        sut.CopyTo( array, 0 );

        array.Should().BeEquivalentTo( items );
    }

    protected abstract ICollection<T> CreateEmptyCollection();
}
