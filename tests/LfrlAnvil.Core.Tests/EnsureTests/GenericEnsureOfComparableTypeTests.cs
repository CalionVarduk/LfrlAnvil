using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using Xunit;

namespace LfrlAnvil.Tests.EnsureTests;

[GenericTestClass( typeof( GenericEnsureTestsData<> ) )]
public abstract class GenericEnsureOfComparableTypeTests<T> : GenericEnsureTests<T>
    where T : IEquatable<T>, IComparable<T>
{
    [Fact]
    public void Equals_ShouldPass_WhenParamIsEqualToValue()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldPass( () => Ensure.Equals( param, param ) );
    }

    [Fact]
    public void Equals_ShouldThrowArgumentException_WhenParamIsNotEqualToValue()
    {
        var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
        ShouldThrowArgumentException( () => Ensure.Equals( param, value ) );
    }

    [Fact]
    public void NotEquals_ShouldPass_WhenParamIsNotEqualToValue()
    {
        var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
        ShouldPass( () => Ensure.NotEquals( param, value ) );
    }

    [Fact]
    public void NotEquals_ShouldThrowArgumentException_WhenParamIsEqualToValue()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldThrowArgumentException( () => Ensure.NotEquals( param, param ) );
    }

    [Fact]
    public void IsGreaterThan_ShouldPass_WhenParamIsGreaterThanValue()
    {
        var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsGreaterThan( param, value ) );
    }

    [Fact]
    public void IsGreaterThan_ShouldThrowArgumentOutOfRangeException_WhenParamIsLessThanValue()
    {
        var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsGreaterThan( param, value ) );
    }

    [Fact]
    public void IsGreaterThan_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToValue()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsGreaterThan( param, param ) );
    }

    [Fact]
    public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsGreaterThanValue()
    {
        var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsGreaterThanOrEqualTo( param, value ) );
    }

    [Fact]
    public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldPass( () => Ensure.IsGreaterThanOrEqualTo( param, param ) );
    }

    [Fact]
    public void IsGreaterThanOrEqualTo_ShouldThrowArgumentOutOfRangeException_WhenParamIsLessThanValue()
    {
        var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsGreaterThanOrEqualTo( param, value ) );
    }

    [Fact]
    public void IsLessThan_ShouldPass_WhenParamIsLessThanValue()
    {
        var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsLessThan( param, value ) );
    }

    [Fact]
    public void IsLessThan_ShouldThrowArgumentOutOfRangeException_WhenParamIsGreaterThanValue()
    {
        var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsLessThan( param, value ) );
    }

    [Fact]
    public void IsLessThan_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToValue()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsLessThan( param, param ) );
    }

    [Fact]
    public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsLessThanValue()
    {
        var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsLessThanOrEqualTo( param, value ) );
    }

    [Fact]
    public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldPass( () => Ensure.IsLessThanOrEqualTo( param, param ) );
    }

    [Fact]
    public void IsLessThanOrEqualTo_ShouldThrowArgumentOutOfRangeException_WhenParamIsGreaterThanValue()
    {
        var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsLessThanOrEqualTo( param, value ) );
    }

    [Fact]
    public void IsInRange_ShouldPass_WhenParamIsBetweenTwoDistinctValues()
    {
        var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldPass( () => Ensure.IsInRange( param, min, max ) );
    }

    [Fact]
    public void IsInRange_ShouldPass_WhenParamIsEqualToMinValue()
    {
        var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsInRange( param, param, max ) );
    }

    [Fact]
    public void IsInRange_ShouldPass_WhenParamIsEqualToMaxValue()
    {
        var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsInRange( param, min, param ) );
    }

    [Fact]
    public void IsInRange_ShouldPass_WhenParamIsEqualToMinAndMaxValues()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldPass( () => Ensure.IsInRange( param, param, param ) );
    }

    [Fact]
    public void IsInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsLessThanMinValue()
    {
        var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInRange( param, min, max ) );
    }

    [Fact]
    public void IsInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsGreaterThanMaxValue()
    {
        var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInRange( param, min, max ) );
    }

    [Fact]
    public void IsInRange_ShouldThrowArgumentOutOfRangeException_WhenMinIsGreaterThanMax()
    {
        var param = Fixture.CreateNotDefault<T>();
        var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInRange( param, min, max ) );
    }

    [Fact]
    public void IsNotInRange_ShouldPass_WhenParamIsLessThanMinValue()
    {
        var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldPass( () => Ensure.IsNotInRange( param, min, max ) );
    }

    [Fact]
    public void IsNotInRange_ShouldPass_WhenParamIsGreaterThanMaxValue()
    {
        var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldPass( () => Ensure.IsNotInRange( param, min, max ) );
    }

    [Fact]
    public void IsNotInRange_ShouldPass_WhenMinIsGreaterThanMax()
    {
        var param = Fixture.CreateNotDefault<T>();
        var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsNotInRange( param, min, max ) );
    }

    [Fact]
    public void IsNotInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsBetweenTwoDistinctValues()
    {
        var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInRange( param, min, max ) );
    }

    [Fact]
    public void IsNotInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMinValue()
    {
        var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInRange( param, param, max ) );
    }

    [Fact]
    public void IsNotInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMaxValue()
    {
        var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInRange( param, min, param ) );
    }

    [Fact]
    public void IsNotInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMinAndMaxValues()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInRange( param, param, param ) );
    }

    [Fact]
    public void IsInExclusiveRange_ShouldPass_WhenParamIsBetweenTwoDistinctValues()
    {
        var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldPass( () => Ensure.IsInExclusiveRange( param, min, max ) );
    }

    [Fact]
    public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMinValue()
    {
        var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, param, max ) );
    }

    [Fact]
    public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMaxValue()
    {
        var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, min, param ) );
    }

    [Fact]
    public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMinAndMaxValues()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, param, param ) );
    }

    [Fact]
    public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsLessThanMinValue()
    {
        var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, min, max ) );
    }

    [Fact]
    public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsGreaterThanMaxValue()
    {
        var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, min, max ) );
    }

    [Fact]
    public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenMinIsGreaterThanMax()
    {
        var param = Fixture.CreateNotDefault<T>();
        var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, min, max ) );
    }

    [Fact]
    public void IsNotInExclusiveRange_ShouldPass_WhenParamIsLessThanMinValue()
    {
        var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldPass( () => Ensure.IsNotInExclusiveRange( param, min, max ) );
    }

    [Fact]
    public void IsNotInExclusiveRange_ShouldPass_WhenParamIsGreaterThanMaxValue()
    {
        var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldPass( () => Ensure.IsNotInExclusiveRange( param, min, max ) );
    }

    [Fact]
    public void IsNotInExclusiveRange_ShouldPass_WhenMinIsGreaterThanMax()
    {
        var param = Fixture.CreateNotDefault<T>();
        var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsNotInExclusiveRange( param, min, max ) );
    }

    [Fact]
    public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMinValue()
    {
        var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsNotInExclusiveRange( param, param, max ) );
    }

    [Fact]
    public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMaxValue()
    {
        var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        ShouldPass( () => Ensure.IsNotInExclusiveRange( param, min, param ) );
    }

    [Fact]
    public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMinAndMaxValues()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldPass( () => Ensure.IsNotInExclusiveRange( param, param, param ) );
    }

    [Fact]
    public void IsNotInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsBetweenTwoDistinctValues()
    {
        var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
        ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInExclusiveRange( param, min, max ) );
    }

    [Fact]
    public void Contains_ShouldPass_WhenEnumerableContainsElement()
    {
        var value = Fixture.Create<T>();
        var param = Fixture.CreateMany<T>().Append( value );
        ShouldPass( () => Ensure.Contains( param, value ) );
    }

    [Fact]
    public void Contains_ShouldThrowArgumentException_WhenEnumerableDoesntContainElement()
    {
        var value = Fixture.Create<T>();
        var param = Fixture.CreateMany<T>().Except( new[] { value } );
        ShouldThrowArgumentException( () => Ensure.Contains( param, value ) );
    }

    [Fact]
    public void NotContains_ShouldPass_WhenEnumerableDoesntContainElement()
    {
        var value = Fixture.Create<T>();
        var param = Fixture.CreateMany<T>().Except( new[] { value } );
        ShouldPass( () => Ensure.NotContains( param, value ) );
    }

    [Fact]
    public void NotContains_ShouldThrowArgumentException_WhenEnumerableContainsElement()
    {
        var value = Fixture.Create<T>();
        var param = Fixture.CreateMany<T>().Concat( new[] { value } );
        ShouldThrowArgumentException( () => Ensure.NotContains( param, value ) );
    }

    [Fact]
    public void IsOrdered_ShouldPass_ForEmptyCollection()
    {
        var param = Enumerable.Empty<T>();
        ShouldPass( () => Ensure.IsOrdered( param ) );
    }

    [Fact]
    public void IsOrdered_ShouldPass_ForCollectionWithOneElement()
    {
        var param = Fixture.CreateMany<T>( 1 );
        ShouldPass( () => Ensure.IsOrdered( param ) );
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetIsOrderedPassData ) )]
    public void IsOrdered_ShouldPass_ForOrderedCollection(IEnumerable<T> param)
    {
        ShouldPass( () => Ensure.IsOrdered( param ) );
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetIsOrderedThrowData ) )]
    public void IsOrdered_ShouldThrowArgumentException_ForUnorderedCollection(IEnumerable<T> param)
    {
        ShouldThrowArgumentException( () => Ensure.IsOrdered( param ) );
    }
}
