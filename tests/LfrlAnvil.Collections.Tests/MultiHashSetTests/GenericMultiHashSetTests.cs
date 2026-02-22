using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Collections.Tests.MultiHashSetTests;

[GenericTestClass( typeof( GenericMultiHashSetTestsData<> ) )]
public abstract class GenericMultiHashSetTests<T> : GenericCollectionTestsBase<Pair<T, int>>
    where T : notnull
{
    protected GenericMultiHashSetTests()
    {
        Fixture.Customize<Pair<T, int>>( (_, _) => f => Pair.Create( f.Create<T>(), 1 ) );
    }

    [Fact]
    public void Ctor_ShouldCreateEmptySet()
    {
        var sut = new MultiHashSet<T>();

        Assertion.All(
                sut.FullCount.TestEquals( 0 ),
                sut.Comparer.TestEquals( EqualityComparer<T>.Default ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithExplicitComparer()
    {
        var comparer = EqualityComparerFactory<T>.Create( (a, b) => a!.Equals( b ) );
        var sut = new MultiHashSet<T>( comparer );
        sut.Comparer.TestRefEquals( comparer ).Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemWithMultiplicityEqualToOne()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.Add( item );

        Assertion.All(
                result.TestEquals( 1 ),
                sut.FullCount.TestEquals( 1 ),
                sut.Count.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemWithMultiplicityEqualToOne_WhenOtherItemExists()
    {
        var (other, item) = Fixture.CreateManyDistinct<T>( count: 2 );

        var sut = new MultiHashSet<T> { other };

        var result = sut.Add( item );

        Assertion.All(
                result.TestEquals( 1 ),
                sut.FullCount.TestEquals( 2 ),
                sut.Count.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldIncreaseMultiplicityOfExistingItemByOne()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T> { item };

        var result = sut.Add( item );

        Assertion.All(
                result.TestEquals( 2 ),
                sut.FullCount.TestEquals( 2 ),
                sut.Count.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldIncreaseMultiplicityOfExistingItemByOne_WhenOtherItemExists()
    {
        var (other, item) = Fixture.CreateManyDistinct<T>( count: 2 );

        var sut = new MultiHashSet<T>
        {
            other,
            item
        };

        var result = sut.Add( item );

        Assertion.All(
                result.TestEquals( 2 ),
                sut.FullCount.TestEquals( 3 ),
                sut.Count.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldThrowOverflowException_WhenItemMultiplicityIsTooLarge()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, int.MaxValue );

        var action = Lambda.Of( () => sut.Add( item ) );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<OverflowException>(),
                sut.Count.TestEquals( 1 ),
                sut.FullCount.TestEquals( int.MaxValue ) ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void AddMany_ShouldAddNewItemWithMultiplicityEqualToCount(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.AddMany( item, count );

        Assertion.All(
                result.TestEquals( count ),
                sut.FullCount.TestEquals( count ),
                sut.Count.TestEquals( 1 ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void AddMany_ShouldAddNewItemWithMultiplicityEqualToCount_WhenOtherItemExists(int count)
    {
        var (other, item) = Fixture.CreateManyDistinct<T>( count: 2 );

        var sut = new MultiHashSet<T> { other };

        var result = sut.AddMany( item, count );

        Assertion.All(
                result.TestEquals( count ),
                sut.FullCount.TestEquals( 1 + count ),
                sut.Count.TestEquals( 2 ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void AddMany_ShouldIncreaseMultiplicityOfExistingItemByCount(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T> { item };

        var result = sut.AddMany( item, count );

        Assertion.All(
                result.TestEquals( 1 + count ),
                sut.FullCount.TestEquals( 1 + count ),
                sut.Count.TestEquals( 1 ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void AddMany_ShouldIncreaseMultiplicityOfExistingItemByCount_WhenOtherItemExists(int count)
    {
        var (other, item) = Fixture.CreateManyDistinct<T>( count: 2 );

        var sut = new MultiHashSet<T>
        {
            other,
            item
        };

        var result = sut.AddMany( item, count );

        Assertion.All(
                result.TestEquals( 1 + count ),
                sut.FullCount.TestEquals( 2 + count ),
                sut.Count.TestEquals( 2 ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void AddMany_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var action = Lambda.Of( () => sut.AddMany( item, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void AddMany_ShouldThrowOverflowException_WhenItemMultiplicityIsTooLarge()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, int.MaxValue );

        var action = Lambda.Of( () => sut.AddMany( item, 1 ) );

        action.Test( exc => exc.TestType().Exact<OverflowException>() ).Go();
    }

    [Fact]
    public void Remove_ShouldReturnMinusOne_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.Remove( item );

        result.TestEquals( -1 ).Go();
    }

    [Fact]
    public void Remove_ShouldDecreaseMultiplicityOfExistingItemByOne()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, 2 );

        var result = sut.Remove( item );

        Assertion.All(
                result.TestEquals( 1 ),
                sut.FullCount.TestEquals( 1 ),
                sut.Count.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveExistingItem_WhenItsMultiplicityIsEqualToOne()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T> { item };

        var result = sut.Remove( item );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.FullCount.TestEquals( 0 ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void RemoveMany_ShouldReturnMinusOne_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.RemoveMany( item, 1 );

        result.TestEquals( -1 ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void RemoveMany_ShouldDecreaseMultiplicityOfExistingItemByCount(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, 4 );

        var result = sut.RemoveMany( item, count );

        Assertion.All(
                result.TestEquals( 4 - count ),
                sut.FullCount.TestEquals( 4 - count ),
                sut.Count.TestEquals( 1 ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void RemoveMany_ShouldRemoveExistingItem_WhenItsMultiplicityIsLessThanOrEqualToRemoveCount(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, 1 );

        var result = sut.RemoveMany( item, count );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.FullCount.TestEquals( 0 ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void RemoveMany_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var action = Lambda.Of( () => sut.RemoveMany( item, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void RemoveAll_ShouldRemoveAnItemAndReturnItsOldMultiplicity(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, count );

        var result = sut.RemoveAll( item );

        Assertion.All(
                result.TestEquals( count ),
                sut.FullCount.TestEquals( 0 ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void RemoveAll_ShouldReturnZero_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.RemoveAll( item );

        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );

        var sut = new MultiHashSet<T>();

        foreach ( var item in items )
            sut.Add( item );

        sut.Clear();

        Assertion.All(
                sut.FullCount.TestEquals( 0 ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void ExceptWith_ShouldClearSet_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.ExceptWith( sut );

        sut.Count.TestEquals( 0 ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetExceptWithData ) )]
    public void ExceptWith_ShouldModifySetCorrectly(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        IEnumerable<Pair<T, int>> expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.ExceptWith( other );

        sut.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void UnionWith_ShouldDoNothing_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.UnionWith( sut );

        sut.TestSetEqual( items ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetUnionWithData ) )]
    public void UnionWith_ShouldModifySetCorrectly(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        IEnumerable<Pair<T, int>> expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.UnionWith( other );

        sut.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void IntersectWith_ShouldDoNothing_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.IntersectWith( sut );

        sut.TestSetEqual( items ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIntersectWithData ) )]
    public void IntersectWith_ShouldModifySetCorrectly(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        IEnumerable<Pair<T, int>> expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.IntersectWith( other );

        sut.TestSetEqual( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIntersectWithData ) )]
    public void IntersectWith_ShouldModifySetCorrectly_WhenOtherIsMultiSet(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        IEnumerable<Pair<T, int>> expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var otherSet = new MultiHashSet<T>( sut.Comparer );
        foreach ( var (item, multiplicity) in other.Where( x => x.Second > 0 ) )
            otherSet.AddMany( item, multiplicity );

        sut.IntersectWith( otherSet );

        sut.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void SymmetricExceptWith_ShouldClearSet_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.SymmetricExceptWith( sut );

        sut.Count.TestEquals( 0 ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetSymmetricExceptWithData ) )]
    public void SymmetricExceptWith_ShouldModifySetCorrectly(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        IEnumerable<Pair<T, int>> expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.SymmetricExceptWith( other );

        sut.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void Overlaps_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.Overlaps( sut );

        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetOverlapsData ) )]
    public void Overlaps_ShouldReturnCorrectResult(IEnumerable<Pair<T, int>> items, IEnumerable<Pair<T, int>> other, bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.Overlaps( other );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SetEquals_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.SetEquals( sut );

        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult(IEnumerable<Pair<T, int>> items, IEnumerable<Pair<T, int>> other, bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.SetEquals( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult_WhenOtherIsMultiSet(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var otherSet = new MultiHashSet<T>( sut.Comparer );
        foreach ( var (item, multiplicity) in other.Where( x => x.Second > 0 ) )
            otherSet.AddMany( item, multiplicity );

        var result = sut.SetEquals( otherSet );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IsSupersetOf_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsSupersetOf( sut );

        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsSupersetOfData ) )]
    public void IsSupersetOf_ShouldReturnCorrectResult(IEnumerable<Pair<T, int>> items, IEnumerable<Pair<T, int>> other, bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsSupersetOf( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsSupersetOfData ) )]
    public void IsSupersetOf_ShouldReturnCorrectResult_WhenOtherIsMultiSet(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var otherSet = new MultiHashSet<T>( sut.Comparer );
        foreach ( var (item, multiplicity) in other.Where( x => x.Second > 0 ) )
            otherSet.AddMany( item, multiplicity );

        var result = sut.IsSupersetOf( otherSet );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IsProperSupersetOf_ShouldReturnFalse_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsProperSupersetOf( sut );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsProperSupersetOfData ) )]
    public void IsProperSupersetOf_ShouldReturnCorrectResult(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsProperSupersetOf( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsProperSupersetOfData ) )]
    public void IsProperSupersetOf_ShouldReturnCorrectResult_WhenOtherIsMultiSet(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var otherSet = new MultiHashSet<T>( sut.Comparer );
        foreach ( var (item, multiplicity) in other.Where( x => x.Second > 0 ) )
            otherSet.AddMany( item, multiplicity );

        var result = sut.IsProperSupersetOf( otherSet );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IsSubsetOf_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsSubsetOf( sut );

        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsSubsetOfData ) )]
    public void IsSubsetOf_ShouldReturnCorrectResult(IEnumerable<Pair<T, int>> items, IEnumerable<Pair<T, int>> other, bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsSubsetOf( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsSubsetOfData ) )]
    public void IsSubsetOf_ShouldReturnCorrectResult_WhenOtherIsMultiSet(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var otherSet = new MultiHashSet<T>( sut.Comparer );
        foreach ( var (item, multiplicity) in other.Where( x => x.Second > 0 ) )
            otherSet.AddMany( item, multiplicity );

        var result = sut.IsSubsetOf( otherSet );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IsProperSubsetOf_ShouldReturnFalse_WhenAppliedToSelf()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsProperSubsetOf( sut );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsProperSubsetOfData ) )]
    public void IsProperSubsetOf_ShouldReturnCorrectResult(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsProperSubsetOf( other );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsProperSubsetOfData ) )]
    public void IsProperSubsetOf_ShouldReturnCorrectResult_WhenOtherIsMultiSet(
        IEnumerable<Pair<T, int>> items,
        IEnumerable<Pair<T, int>> other,
        bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var otherSet = new MultiHashSet<T>( sut.Comparer );
        foreach ( var (item, multiplicity) in other.Where( x => x.Second > 0 ) )
            otherSet.AddMany( item, multiplicity );

        var result = sut.IsProperSubsetOf( otherSet );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenItemExists()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T> { item };

        var result = sut.Contains( item );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.Contains( item );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetContainsData ) )]
    public void Contains_WithItemAndMultiplicity_ShouldReturnTrue_WhenItemExistsWithCorrectMultiplicity(
        int existingMultiplicity,
        int checkedMultiplicity,
        bool expected)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, existingMultiplicity );

        var result = sut.Contains( item, checkedMultiplicity );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Contains_WithItemAndMultiplicity_ShouldReturnFalse_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();
        var multiplicity = Fixture.Create<int>();

        var sut = new MultiHashSet<T>();

        var result = sut.Contains( item, multiplicity );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetContainsData ) )]
    public void Contains_WithPair_ShouldReturnTrue_WhenItemExistsWithCorrectMultiplicity(
        int existingMultiplicity,
        int checkedMultiplicity,
        bool expected)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, existingMultiplicity );

        var result = sut.Contains( Pair.Create( item, checkedMultiplicity ) );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Contains_WithPair_ShouldReturnFalse_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();
        var multiplicity = Fixture.Create<int>();

        var sut = new MultiHashSet<T>();

        var result = sut.Contains( Pair.Create( item, multiplicity ) );

        result.TestFalse().Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void GetMultiplicity_ShouldReturnCorrectResult_WhenItemExists(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, count );

        var result = sut.GetMultiplicity( item );

        result.TestEquals( count ).Go();
    }

    [Fact]
    public void GetMultiplicity_ShouldReturnZero_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.GetMultiplicity( item );

        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void SetMultiplicity_ShouldReturnZeroAndDoNothing_WhenItemDoesntExistAndValueIsZero()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.SetMultiplicity( item, 0 );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.FullCount.TestEquals( 0 ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void SetMultiplicity_ShouldReturnZeroAndAddNewItem_WhenItemDoesntExist(int value)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.SetMultiplicity( item, value );

        Assertion.All(
                result.TestEquals( 0 ),
                sut.Count.TestEquals( 1 ),
                sut.FullCount.TestEquals( value ),
                sut.GetMultiplicity( item ).TestEquals( value ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void SetMultiplicity_ShouldReturnMultiplicityOfExistingItemAndDoNothing_WhenNewMultiplicityIsTheSame(int value)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, value );

        var result = sut.SetMultiplicity( item, value );

        Assertion.All(
                result.TestEquals( value ),
                sut.Count.TestEquals( 1 ),
                sut.FullCount.TestEquals( value ),
                sut.GetMultiplicity( item ).TestEquals( value ) )
            .Go();
    }

    [Theory]
    [InlineData( 2, 3 )]
    [InlineData( 2, 4 )]
    [InlineData( 2, 5 )]
    [InlineData( 6, 3 )]
    [InlineData( 6, 4 )]
    [InlineData( 6, 5 )]
    public void SetMultiplicity_ShouldReturnOldMultiplicityOfExistingItemAndUpdateMultiplicity(int oldMultiplicity, int newMultiplicity)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, oldMultiplicity );

        var result = sut.SetMultiplicity( item, newMultiplicity );

        Assertion.All(
                result.TestEquals( oldMultiplicity ),
                sut.Count.TestEquals( 1 ),
                sut.FullCount.TestEquals( newMultiplicity ),
                sut.GetMultiplicity( item ).TestEquals( newMultiplicity ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void SetMultiplicity_ShouldReturnMultiplicityOfExistingItemRemoveIt_WhenNewMultiplicityIsZero(int oldMultiplicity)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, oldMultiplicity );

        var result = sut.SetMultiplicity( item, 0 );

        Assertion.All(
                result.TestEquals( oldMultiplicity ),
                sut.Count.TestEquals( 0 ),
                sut.FullCount.TestEquals( 0 ),
                sut.GetMultiplicity( item ).TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void DistinctItems_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );

        var sut = new MultiHashSet<T>();

        for ( var i = 0; i < items.Length; ++i )
            sut.AddMany( items[i], i + 1 );

        sut.DistinctItems.TestSetEqual( items ).Go();
    }

    [Fact]
    public void Items_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );

        var expected = new[] { items[0], items[1], items[1], items[2], items[2], items[2] };

        var sut = new MultiHashSet<T>();

        for ( var i = 0; i < items.Length; ++i )
            sut.AddMany( items[i], i + 1 );

        Assertion.All( sut.Items.Count().TestEquals( expected.Length ), sut.Items.TestSetEqual( expected ) ).Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateManyDistinct<T>( count: 3 );
        var expected = new[] { Pair.Create( items[0], 1 ), Pair.Create( items[1], 2 ), Pair.Create( items[2], 3 ) }.AsEnumerable();

        var sut = new MultiHashSet<T>();

        for ( var i = 0; i < items.Length; ++i )
            sut.AddMany( items[i], i + 1 );

        sut.TestSetEqual( expected ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void ISetAdd_ShouldReturnTrueAndAddItemCorrectly_WhenSecondIsGreaterThanZero(int multiplicity)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T> { item };
        ISet<Pair<T, int>> set = sut;

        var result = set.Add( Pair.Create( item, multiplicity ) );

        Assertion.All(
                result.TestTrue(),
                sut.FullCount.TestEquals( multiplicity + 1 ),
                sut.Count.TestEquals( 1 ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void ISetAdd_ShouldThrowArgumentOutOfRangeException_WhenSecondIsLessThanOne(int multiplicity)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T> { item };
        ISet<Pair<T, int>> set = sut;

        var action = Lambda.Of( () => set.Add( Pair.Create( item, multiplicity ) ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    protected sealed override ICollection<Pair<T, int>> CreateEmptyCollection()
    {
        return new MultiHashSet<T>();
    }
}
