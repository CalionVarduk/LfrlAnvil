using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Collections.Tests.SequentialHashSetTests;

[GenericTestClass( typeof( GenericSequentialHashSetTestsData<> ) )]
public abstract class GenericSequentialHashSetTests<T> : GenericCollectionTestsBase<T>
    where T : notnull
{
    [Fact]
    public void Ctor_ShouldCreateEmpty()
    {
        var sut = new SequentialHashSet<T>();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().Be( EqualityComparer<T>.Default );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateEmpty_WithExplicitComparer()
    {
        var comparer = EqualityComparerFactory<T>.Create( (a, b) => a!.Equals( b ) );
        var sut = new SequentialHashSet<T>( comparer );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().Be( comparer );
        }
    }

    [Fact]
    public void Add_ShouldReturnTrueAndAddNewItem_WhenSetIsEmpty()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T>();

        var result = sut.Add( item );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Contains( item ).Should().BeTrue();
            result.Should().BeTrue();
        }
    }

    [Fact]
    public void Add_ShouldReturnTrueAndAddNewItem_WhenSetInNotEmpty()
    {
        var items = Fixture.CreateDistinctCollection<T>( 2 );
        var sut = new SequentialHashSet<T> { items[0] };

        var result = sut.Add( items[1] );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Contains( items[1] ).Should().BeTrue();
            result.Should().BeTrue();
        }
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenItemAlreadyExists()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T> { item };

        var result = sut.Add( item );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            result.Should().BeFalse();
        }
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenSetIsEmpty()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T>();

        var result = sut.Remove( item );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveExistingItem_WhenSetHasOneItem()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T> { item };

        var result = sut.Remove( item );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem()
    {
        var items = Fixture.CreateDistinctCollection<T>( 2 );
        var sut = new SequentialHashSet<T>
        {
            items[0],
            items[1]
        };

        var result = sut.Remove( items[0] );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.Contains( items[1] ).Should().BeTrue();
        }
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenItemExists()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T> { item };

        var result = sut.Contains( item );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T>();

        var result = sut.Contains( item );

        result.Should().BeFalse();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();

        foreach ( var item in items )
            sut.Add( item );

        sut.Clear();

        sut.Count.Should().Be( 0 );
    }

    [Fact]
    public void ExceptWith_ShouldClearSet_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.ExceptWith( sut );

        sut.Should().HaveCount( 0 );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetExceptWithData ) )]
    public void ExceptWith_ShouldModifySetCorrectly(IEnumerable<T> items, IEnumerable<T> other, IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.ExceptWith( other );

        sut.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void UnionWith_ShouldDoNothing_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.UnionWith( sut );

        sut.Should().BeSequentiallyEqualTo( items );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetUnionWithData ) )]
    public void UnionWith_ShouldModifySetCorrectly(IEnumerable<T> items, IEnumerable<T> other, IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.UnionWith( other );

        sut.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void IntersectWith_ShouldDoNothing_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.IntersectWith( sut );

        sut.Should().BeSequentiallyEqualTo( items );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIntersectWithData ) )]
    public void IntersectWith_ShouldModifySetCorrectly(IEnumerable<T> items, IEnumerable<T> other, IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.IntersectWith( other );

        sut.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIntersectWithData ) )]
    public void IntersectWith_ShouldModifySetCorrectly_WhenOtherIsHashSet(
        IEnumerable<T> items,
        IEnumerable<T> other,
        IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var otherSet = new HashSet<T>( other, sut.Comparer );

        sut.IntersectWith( otherSet );

        sut.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIntersectWithData ) )]
    public void IntersectWith_ShouldModifySetCorrectly_WhenOtherIsSequentialHashSet(
        IEnumerable<T> items,
        IEnumerable<T> other,
        IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var otherSet = new SequentialHashSet<T>( sut.Comparer );
        foreach ( var item in other )
            otherSet.Add( item );

        sut.IntersectWith( otherSet );

        sut.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void SymmetricExceptWith_ShouldClearSet_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.SymmetricExceptWith( sut );

        sut.Should().HaveCount( 0 );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetSymmetricExceptWithData ) )]
    public void SymmetricExceptWith_ShouldModifySetCorrectly(IEnumerable<T> items, IEnumerable<T> other, IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.SymmetricExceptWith( other );

        sut.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void Overlaps_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.Overlaps( sut );

        result.Should().BeTrue();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetOverlapsData ) )]
    public void Overlaps_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.Overlaps( other );

        result.Should().Be( expected );
    }

    [Fact]
    public void SetEquals_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.SetEquals( sut );

        result.Should().BeTrue();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.SetEquals( other );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult_WhenOtherIsHashSet(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var otherSet = new HashSet<T>( other, sut.Comparer );

        var result = sut.SetEquals( otherSet );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult_WhenOtherIsSequentialHashSet(
        IEnumerable<T> items,
        IEnumerable<T> other,
        bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var otherSet = new SequentialHashSet<T>( sut.Comparer );
        foreach ( var item in other )
            otherSet.Add( item );

        var result = sut.SetEquals( otherSet );

        result.Should().Be( expected );
    }

    [Fact]
    public void IsSupersetOf_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsSupersetOf( sut );

        result.Should().BeTrue();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsSupersetOfData ) )]
    public void IsSupersetOf_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsSupersetOf( other );

        result.Should().Be( expected );
    }

    [Fact]
    public void IsProperSupersetOf_ShouldReturnFalse_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsProperSupersetOf( sut );

        result.Should().BeFalse();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsProperSupersetOfData ) )]
    public void IsProperSupersetOf_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsProperSupersetOf( other );

        result.Should().Be( expected );
    }

    [Fact]
    public void IsSubsetOf_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsSubsetOf( sut );

        result.Should().BeTrue();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsSubsetOfData ) )]
    public void IsSubsetOf_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsSubsetOf( other );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsSubsetOfData ) )]
    public void IsSubsetOf_ShouldReturnCorrectResult_WhenOtherIsHashSet(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var otherSet = new HashSet<T>( other, sut.Comparer );

        var result = sut.IsSubsetOf( otherSet );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsSubsetOfData ) )]
    public void IsSubsetOf_ShouldReturnCorrectResult_WhenOtherIsSequentialHashSet(
        IEnumerable<T> items,
        IEnumerable<T> other,
        bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var otherSet = new SequentialHashSet<T>( sut.Comparer );
        foreach ( var item in other )
            otherSet.Add( item );

        var result = sut.IsSubsetOf( otherSet );

        result.Should().Be( expected );
    }

    [Fact]
    public void IsProperSubsetOf_ShouldReturnFalse_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsProperSubsetOf( sut );

        result.Should().BeFalse();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsProperSubsetOfData ) )]
    public void IsProperSubsetOf_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsProperSubsetOf( other );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsProperSubsetOfData ) )]
    public void IsProperSubsetOf_ShouldReturnCorrectResult_WhenOtherIsHashSet(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var otherSet = new HashSet<T>( other, sut.Comparer );

        var result = sut.IsProperSubsetOf( otherSet );

        result.Should().Be( expected );
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsProperSubsetOfData ) )]
    public void IsProperSubsetOf_ShouldReturnCorrectResult_WhenOtherIsSequentialHashSet(
        IEnumerable<T> items,
        IEnumerable<T> other,
        bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var otherSet = new SequentialHashSet<T>( sut.Comparer );
        foreach ( var item in other )
            otherSet.Add( item );

        var result = sut.IsProperSubsetOf( otherSet );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectAndOrderedResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 10 );

        var initialItems = items.Take( 7 );
        var itemsToRemove = new[] { items[0], items[2], items[6] };
        var itemsToAddLast = items.Skip( 7 );

        var expected = new[] { items[1], items[3], items[4], items[5], items[7], items[8], items[9] };

        var sut = new SequentialHashSet<T>();

        foreach ( var item in initialItems )
            sut.Add( item );

        foreach ( var item in itemsToRemove )
            sut.Remove( item );

        foreach ( var item in itemsToAddLast )
            sut.Add( item );

        sut.Should().BeSequentiallyEqualTo( expected );
    }

    protected sealed override ICollection<T> CreateEmptyCollection()
    {
        return new SequentialHashSet<T>();
    }
}
