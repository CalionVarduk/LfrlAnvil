using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using Xunit;

namespace LfrlAnvil.Collections.Tests.MultiHashSetTests;

[GenericTestClass( typeof( GenericMultiHashSetTestsData<> ) )]
public abstract class GenericMultiHashSetTests<T> : GenericCollectionTestsBase<Pair<T, int>>
    where T : notnull
{
    protected GenericMultiHashSetTests()
    {
        Fixture.Customize<Pair<T, int>>( c => c.FromFactory( () => Pair.Create( Fixture.Create<T>(), 1 ) ) );
    }

    [Fact]
    public void Ctor_ShouldCreateEmptySet()
    {
        var sut = new MultiHashSet<T>();

        using ( new AssertionScope() )
        {
            sut.FullCount.Should().Be( 0 );
            sut.Comparer.Should().Be( EqualityComparer<T>.Default );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateWithExplicitComparer()
    {
        var comparer = EqualityComparerFactory<T>.Create( (a, b) => a!.Equals( b ) );
        var sut = new MultiHashSet<T>( comparer );
        sut.Comparer.Should().Be( comparer );
    }

    [Fact]
    public void Add_ShouldAddNewItemWithMultiplicityEqualToOne()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.Add( item );

        using ( new AssertionScope() )
        {
            result.Should().Be( 1 );
            sut.FullCount.Should().Be( 1 );
            sut.Count.Should().Be( 1 );
        }
    }

    [Fact]
    public void Add_ShouldAddNewItemWithMultiplicityEqualToOne_WhenOtherItemExists()
    {
        var (other, item) = Fixture.CreateDistinctCollection<T>( 2 );

        var sut = new MultiHashSet<T> { other };

        var result = sut.Add( item );

        using ( new AssertionScope() )
        {
            result.Should().Be( 1 );
            sut.FullCount.Should().Be( 2 );
            sut.Count.Should().Be( 2 );
        }
    }

    [Fact]
    public void Add_ShouldIncreaseMultiplicityOfExistingItemByOne()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T> { item };

        var result = sut.Add( item );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut.FullCount.Should().Be( 2 );
            sut.Count.Should().Be( 1 );
        }
    }

    [Fact]
    public void Add_ShouldIncreaseMultiplicityOfExistingItemByOne_WhenOtherItemExists()
    {
        var (other, item) = Fixture.CreateDistinctCollection<T>( 2 );

        var sut = new MultiHashSet<T> { other, item };

        var result = sut.Add( item );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut.FullCount.Should().Be( 3 );
            sut.Count.Should().Be( 2 );
        }
    }

    [Fact]
    public void Add_ShouldThrowOverflowException_WhenItemMultiplicityIsTooLarge()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, int.MaxValue );

        var action = Lambda.Of( () => sut.Add( item ) );

        using ( new AssertionScope() )
        {
            action.Should().ThrowExactly<OverflowException>();
            sut.Count.Should().Be( 1 );
            sut.FullCount.Should().Be( int.MaxValue );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( count );
            sut.FullCount.Should().Be( count );
            sut.Count.Should().Be( 1 );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void AddMany_ShouldAddNewItemWithMultiplicityEqualToCount_WhenOtherItemExists(int count)
    {
        var (other, item) = Fixture.CreateDistinctCollection<T>( 2 );

        var sut = new MultiHashSet<T> { other };

        var result = sut.AddMany( item, count );

        using ( new AssertionScope() )
        {
            result.Should().Be( count );
            sut.FullCount.Should().Be( 1 + count );
            sut.Count.Should().Be( 2 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( 1 + count );
            sut.FullCount.Should().Be( 1 + count );
            sut.Count.Should().Be( 1 );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void AddMany_ShouldIncreaseMultiplicityOfExistingItemByCount_WhenOtherItemExists(int count)
    {
        var (other, item) = Fixture.CreateDistinctCollection<T>( 2 );

        var sut = new MultiHashSet<T> { other, item };

        var result = sut.AddMany( item, count );

        using ( new AssertionScope() )
        {
            result.Should().Be( 1 + count );
            sut.FullCount.Should().Be( 2 + count );
            sut.Count.Should().Be( 2 );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void AddMany_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var action = Lambda.Of( () => sut.AddMany( item, count ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddMany_ShouldThrowOverflowException_WhenItemMultiplicityIsTooLarge()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, int.MaxValue );

        var action = Lambda.Of( () => sut.AddMany( item, 1 ) );

        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void Remove_ShouldReturnMinusOne_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.Remove( item );

        result.Should().Be( -1 );
    }

    [Fact]
    public void Remove_ShouldDecreaseMultiplicityOfExistingItemByOne()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();
        sut.AddMany( item, 2 );

        var result = sut.Remove( item );

        using ( new AssertionScope() )
        {
            result.Should().Be( 1 );
            sut.FullCount.Should().Be( 1 );
            sut.Count.Should().Be( 1 );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveExistingItem_WhenItsMultiplicityIsEqualToOne()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T> { item };

        var result = sut.Remove( item );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.FullCount.Should().Be( 0 );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void RemoveMany_ShouldReturnMinusOne_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.RemoveMany( item, 1 );

        result.Should().Be( -1 );
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

        using ( new AssertionScope() )
        {
            result.Should().Be( 4 - count );
            sut.FullCount.Should().Be( 4 - count );
            sut.Count.Should().Be( 1 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.FullCount.Should().Be( 0 );
            sut.Count.Should().Be( 0 );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void RemoveMany_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(int count)
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var action = Lambda.Of( () => sut.RemoveMany( item, count ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
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

        using ( new AssertionScope() )
        {
            result.Should().Be( count );
            sut.FullCount.Should().Be( 0 );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void RemoveAll_ShouldReturnZero_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.RemoveAll( item );

        result.Should().Be( 0 );
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );

        var sut = new MultiHashSet<T>();

        foreach ( var item in items )
            sut.Add( item );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.FullCount.Should().Be( 0 );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void ExceptWith_ShouldClearSet_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.ExceptWith( sut );

        sut.Should().HaveCount( 0 );
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

        sut.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void UnionWith_ShouldDoNothing_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.UnionWith( sut );

        sut.Should().BeEquivalentTo( items );
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

        sut.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void IntersectWith_ShouldDoNothing_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.IntersectWith( sut );

        sut.Should().BeEquivalentTo( items );
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

        sut.Should().BeEquivalentTo( expected );
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

        sut.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SymmetricExceptWith_ShouldClearSet_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        sut.SymmetricExceptWith( sut );

        sut.Should().HaveCount( 0 );
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

        sut.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void Overlaps_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.Overlaps( sut );

        result.Should().BeTrue();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetOverlapsData ) )]
    public void Overlaps_ShouldReturnCorrectResult(IEnumerable<Pair<T, int>> items, IEnumerable<Pair<T, int>> other, bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.Overlaps( other );

        result.Should().Be( expected );
    }

    [Fact]
    public void SetEquals_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.SetEquals( sut );

        result.Should().BeTrue();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult(IEnumerable<Pair<T, int>> items, IEnumerable<Pair<T, int>> other, bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.SetEquals( other );

        result.Should().Be( expected );
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

        result.Should().Be( expected );
    }

    [Fact]
    public void IsSupersetOf_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsSupersetOf( sut );

        result.Should().BeTrue();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsSupersetOfData ) )]
    public void IsSupersetOf_ShouldReturnCorrectResult(IEnumerable<Pair<T, int>> items, IEnumerable<Pair<T, int>> other, bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsSupersetOf( other );

        result.Should().Be( expected );
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

        result.Should().Be( expected );
    }

    [Fact]
    public void IsProperSupersetOf_ShouldReturnFalse_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsProperSupersetOf( sut );

        result.Should().BeFalse();
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

        result.Should().Be( expected );
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

        result.Should().Be( expected );
    }

    [Fact]
    public void IsSubsetOf_ShouldReturnTrue_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsSubsetOf( sut );

        result.Should().BeTrue();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMultiHashSetTestsData<T>.GetIsSubsetOfData ) )]
    public void IsSubsetOf_ShouldReturnCorrectResult(IEnumerable<Pair<T, int>> items, IEnumerable<Pair<T, int>> other, bool expected)
    {
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsSubsetOf( other );

        result.Should().Be( expected );
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

        result.Should().Be( expected );
    }

    [Fact]
    public void IsProperSubsetOf_ShouldReturnFalse_WhenAppliedToSelf()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 ).Select( (x, i) => Pair.Create( x, i + 1 ) ).ToList();
        var sut = new MultiHashSet<T>();
        foreach ( var (item, multiplicity) in items )
            sut.AddMany( item, multiplicity );

        var result = sut.IsProperSubsetOf( sut );

        result.Should().BeFalse();
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

        result.Should().Be( expected );
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

        result.Should().Be( expected );
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenItemExists()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T> { item };

        var result = sut.Contains( item );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.Contains( item );

        result.Should().BeFalse();
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

        result.Should().Be( expected );
    }

    [Fact]
    public void Contains_WithItemAndMultiplicity_ShouldReturnFalse_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();
        var multiplicity = Fixture.Create<int>();

        var sut = new MultiHashSet<T>();

        var result = sut.Contains( item, multiplicity );

        result.Should().BeFalse();
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

        result.Should().Be( expected );
    }

    [Fact]
    public void Contains_WithPair_ShouldReturnFalse_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();
        var multiplicity = Fixture.Create<int>();

        var sut = new MultiHashSet<T>();

        var result = sut.Contains( Pair.Create( item, multiplicity ) );

        result.Should().BeFalse();
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

        result.Should().Be( count );
    }

    [Fact]
    public void GetMultiplicity_ShouldReturnZero_WhenItemDoesntExist()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.GetMultiplicity( item );

        result.Should().Be( 0 );
    }

    [Fact]
    public void SetMultiplicity_ShouldReturnZeroAndDoNothing_WhenItemDoesntExistAndValueIsZero()
    {
        var item = Fixture.Create<T>();

        var sut = new MultiHashSet<T>();

        var result = sut.SetMultiplicity( item, 0 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.FullCount.Should().Be( 0 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.Count.Should().Be( 1 );
            sut.FullCount.Should().Be( value );
            sut.GetMultiplicity( item ).Should().Be( value );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( value );
            sut.Count.Should().Be( 1 );
            sut.FullCount.Should().Be( value );
            sut.GetMultiplicity( item ).Should().Be( value );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( oldMultiplicity );
            sut.Count.Should().Be( 1 );
            sut.FullCount.Should().Be( newMultiplicity );
            sut.GetMultiplicity( item ).Should().Be( newMultiplicity );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( oldMultiplicity );
            sut.Count.Should().Be( 0 );
            sut.FullCount.Should().Be( 0 );
            sut.GetMultiplicity( item ).Should().Be( 0 );
        }
    }

    [Fact]
    public void DistinctItems_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );

        var sut = new MultiHashSet<T>();

        for ( var i = 0; i < items.Count; ++i )
            sut.AddMany( items[i], i + 1 );

        sut.DistinctItems.Should().BeEquivalentTo( items );
    }

    [Fact]
    public void Items_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );

        var expected = new[]
        {
            items[0],
            items[1],
            items[1],
            items[2],
            items[2],
            items[2]
        };

        var sut = new MultiHashSet<T>();

        for ( var i = 0; i < items.Count; ++i )
            sut.AddMany( items[i], i + 1 );

        sut.Items.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 3 );
        var expected = new[]
            {
                Pair.Create( items[0], 1 ),
                Pair.Create( items[1], 2 ),
                Pair.Create( items[2], 3 )
            }
            .AsEnumerable();

        var sut = new MultiHashSet<T>();

        for ( var i = 0; i < items.Count; ++i )
            sut.AddMany( items[i], i + 1 );

        sut.Should().BeEquivalentTo( expected );
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

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.FullCount.Should().Be( multiplicity + 1 );
            sut.Count.Should().Be( 1 );
        }
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

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    protected sealed override ICollection<Pair<T, int>> CreateEmptyCollection()
    {
        return new MultiHashSet<T>();
    }
}