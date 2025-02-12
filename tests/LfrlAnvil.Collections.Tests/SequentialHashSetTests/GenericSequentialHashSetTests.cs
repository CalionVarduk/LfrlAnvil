using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Collections.Tests.SequentialHashSetTests;

[GenericTestClass( typeof( GenericSequentialHashSetTestsData<> ) )]
public abstract class GenericSequentialHashSetTests<T> : GenericCollectionTestsBase<T>
    where T : notnull
{
    [Fact]
    public void Ctor_ShouldCreateEmpty()
    {
        var sut = new SequentialHashSet<T>();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestEquals( EqualityComparer<T>.Default ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateEmpty_WithExplicitComparer()
    {
        var comparer = EqualityComparerFactory<T>.Create( (a, b) => a!.Equals( b ) );
        var sut = new SequentialHashSet<T>( comparer );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestRefEquals( comparer ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldReturnTrueAndAddNewItem_WhenSetIsEmpty()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T>();

        var result = sut.Add( item );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Contains( item ).TestTrue(),
                result.TestTrue() )
            .Go();
    }

    [Fact]
    public void Add_ShouldReturnTrueAndAddNewItem_WhenSetInNotEmpty()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 2 );
        var sut = new SequentialHashSet<T> { items[0] };

        var result = sut.Add( items[1] );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Contains( items[1] ).TestTrue(),
                result.TestTrue() )
            .Go();
    }

    [Fact]
    public void Add_ShouldReturnFalse_WhenItemAlreadyExists()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T> { item };

        var result = sut.Add( item );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                result.TestFalse() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenSetIsEmpty()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T>();

        var result = sut.Remove( item );

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveExistingItem_WhenSetHasOneItem()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T> { item };

        var result = sut.Remove( item );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 2 );
        var sut = new SequentialHashSet<T>
        {
            items[0],
            items[1]
        };

        var result = sut.Remove( items[0] );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.Contains( items[1] ).TestTrue() )
            .Go();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenItemExists()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T> { item };

        var result = sut.Contains( item );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();
        var sut = new SequentialHashSet<T>();

        var result = sut.Contains( item );

        result.TestFalse().Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();

        foreach ( var item in items )
            sut.Add( item );

        sut.Clear();

        sut.Count.TestEquals( 0 ).Go();
    }

    [Fact]
    public void ExceptWith_ShouldClearSet_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.ExceptWith( sut );

        sut.Count.TestEquals( 0 ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetExceptWithData ) )]
    public void ExceptWith_ShouldModifySetCorrectly(IEnumerable<T> items, IEnumerable<T> other, IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.ExceptWith( other );

        sut.TestSequence( expected ).Go();
    }

    [Fact]
    public void UnionWith_ShouldDoNothing_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.UnionWith( sut );

        sut.TestSequence( items ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetUnionWithData ) )]
    public void UnionWith_ShouldModifySetCorrectly(IEnumerable<T> items, IEnumerable<T> other, IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.UnionWith( other );

        sut.TestSequence( expected ).Go();
    }

    [Fact]
    public void IntersectWith_ShouldDoNothing_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.IntersectWith( sut );

        sut.TestSequence( items ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIntersectWithData ) )]
    public void IntersectWith_ShouldModifySetCorrectly(IEnumerable<T> items, IEnumerable<T> other, IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.IntersectWith( other );

        sut.TestSequence( expected ).Go();
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

        sut.TestSequence( expected ).Go();
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

        sut.TestSequence( expected ).Go();
    }

    [Fact]
    public void SymmetricExceptWith_ShouldClearSet_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.SymmetricExceptWith( sut );

        sut.Count.TestEquals( 0 ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetSymmetricExceptWithData ) )]
    public void SymmetricExceptWith_ShouldModifySetCorrectly(IEnumerable<T> items, IEnumerable<T> other, IEnumerable<T> expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        sut.SymmetricExceptWith( other );

        sut.TestSequence( expected ).Go();
    }

    [Fact]
    public void Overlaps_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.Overlaps( sut );

        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetOverlapsData ) )]
    public void Overlaps_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.Overlaps( other );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SetEquals_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.SetEquals( sut );

        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.SetEquals( other );

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IsSupersetOf_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsSupersetOf( sut );

        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsSupersetOfData ) )]
    public void IsSupersetOf_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsSupersetOf( other );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IsProperSupersetOf_ShouldReturnFalse_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsProperSupersetOf( sut );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsProperSupersetOfData ) )]
    public void IsProperSupersetOf_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsProperSupersetOf( other );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IsSubsetOf_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsSubsetOf( sut );

        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsSubsetOfData ) )]
    public void IsSubsetOf_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsSubsetOf( other );

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IsProperSubsetOf_ShouldReturnFalse_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsProperSubsetOf( sut );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericSequentialHashSetTestsData<T>.GetIsProperSubsetOfData ) )]
    public void IsProperSubsetOf_ShouldReturnCorrectResult(IEnumerable<T> items, IEnumerable<T> other, bool expected)
    {
        var sut = new SequentialHashSet<T>();
        foreach ( var item in items )
            sut.Add( item );

        var result = sut.IsProperSubsetOf( other );

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectAndOrderedResult()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 10 );

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

        sut.TestSequence( expected ).Go();
    }

    protected sealed override ICollection<T> CreateEmptyCollection()
    {
        return new SequentialHashSet<T>();
    }
}
