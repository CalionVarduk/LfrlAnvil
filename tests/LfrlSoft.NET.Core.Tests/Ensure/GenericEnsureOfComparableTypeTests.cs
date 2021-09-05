using System;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Ensure
{
    public abstract class GenericEnsureOfComparableTypeTests<T> : GenericEnsureTests<T>
        where T : IEquatable<T>, IComparable<T>
    {
        [Fact]
        public void Equals_ShouldPass_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Core.Ensure.Equals( param, param ) );
        }

        [Fact]
        public void Equals_ShouldThrow_WhenParamIsNotEqualToValue()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldThrow( () => Core.Ensure.Equals( param, value ) );
        }

        [Fact]
        public void NotEquals_ShouldPass_WhenParamIsNotEqualToValue()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.NotEquals( param, value ) );
        }

        [Fact]
        public void NotEquals_ShouldThrow_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Core.Ensure.NotEquals( param, param ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldPass_WhenParamIsGreaterThanValue()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsGreaterThan( param, value ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsLessThanValue()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsGreaterThan( param, value ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsGreaterThan( param, param ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsGreaterThanValue()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsGreaterThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsGreaterThanOrEqualTo( param, param ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldThrow_WhenParamIsLessThanValue()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsGreaterThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsLessThan_ShouldPass_WhenParamIsLessThanValue()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsLessThan( param, value ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsGreaterThanValue()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsLessThan( param, value ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsLessThan( param, param ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsLessThanValue()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsLessThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsLessThanOrEqualTo( param, param ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldThrow_WhenParamIsGreaterThanValue()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsLessThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsBetweenTwoDistinctValues()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsInRange( param, min, max ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsEqualToMinValue()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsInRange( param, param, max ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsEqualToMaxValue()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsInRange( param, min, param ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsEqualToMinAndMaxValues()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsInRange( param, param, param ) );
        }

        [Fact]
        public void IsInRange_ShouldThrow_WhenParamIsLessThanMinValue()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInRange( param, min, max ) );
        }

        [Fact]
        public void IsInRange_ShouldThrow_WhenParamIsGreaterThanMaxValue()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInRange( param, min, max ) );
        }

        [Fact]
        public void IsInRange_ShouldThrow_WhenMinIsGreaterThanMax()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInRange( param, min, max ) );
        }

        [Fact]
        public void IsNotInRange_ShouldPass_WhenParamIsLessThanMinValue()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsNotInRange( param, min, max ) );
        }

        [Fact]
        public void IsNotInRange_ShouldPass_WhenParamIsGreaterThanMaxValue()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsNotInRange( param, min, max ) );
        }

        [Fact]
        public void IsNotInRange_ShouldPass_WhenMinIsGreaterThanMax()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsNotInRange( param, min, max ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrow_WhenParamIsBetweenTwoDistinctValues()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInRange( param, min, max ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrow_WhenParamIsEqualToMinValue()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInRange( param, param, max ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrow_WhenParamIsEqualToMaxValue()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInRange( param, min, param ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrow_WhenParamIsEqualToMinAndMaxValues()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInRange( param, param, param ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldPass_WhenParamIsBetweenTwoDistinctValues()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsInExclusiveRange( param, min, max ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenParamIsEqualToMinValue()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, param, max ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenParamIsEqualToMaxValue()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, min, param ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldPass_WhenParamIsEqualToMinAndMaxValues()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, param, param ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenParamIsLessThanMinValue()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, min, max ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenParamIsGreaterThanMaxValue()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, min, max ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenMinIsGreaterThanMax()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, min, max ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsLessThanMinValue()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, min, max ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsGreaterThanMaxValue()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, min, max ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenMinIsGreaterThanMax()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, min, max ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMinValue()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, param, max ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMaxValue()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, min, param ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMinAndMaxValues()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, param, param ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldThrow_WhenParamIsBetweenTwoDistinctValues()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInExclusiveRange( param, min, max ) );
        }

        [Fact]
        public void Contains_ShouldPass_WhenEnumerableContainsElement()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Core.Ensure.Contains( param, value ) );
        }

        [Fact]
        public void Contains_ShouldThrow_WhenEnumerableDoesntContainElement()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrow( () => Core.Ensure.Contains( param, value ) );
        }

        [Fact]
        public void NotContains_ShouldPass_WhenEnumerableDoesntContainElement()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldPass( () => Core.Ensure.NotContains( param, value ) );
        }

        [Fact]
        public void NotContains_ShouldThrow_WhenEnumerableContainsElement()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Concat( new[] { value } );
            ShouldThrow( () => Core.Ensure.NotContains( param, value ) );
        }
    }
}
