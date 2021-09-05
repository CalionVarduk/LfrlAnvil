using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Ensure
{
    [GenericTestClass( typeof( GenericEnsureTestsData<> ) )]
    public abstract class GenericEnsureTests<T> : EnsureTestsBase
    {
        protected readonly IEqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;
        protected readonly IComparer<T> Comparer = Comparer<T>.Default;

        [Fact]
        public void IsDefault_ShouldPass_WhenParamIsDefault()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldPass( () => Core.Ensure.IsDefault( param ) );
        }

        [Fact]
        public void IsDefault_ShouldThrow_WhenParamIsNotDefault()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow( () => Core.Ensure.IsDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldPass_WhenParamIsNotDefault()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsNotDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldThrow_WhenParamIsDefault()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldThrow( () => Core.Ensure.IsNotDefault( param ) );
        }

        [Fact]
        public void IsOfType_ShouldThrow_WhenTypesDontMatch()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Core.Ensure.IsOfType<object>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldPass_WhenTypesDontMatch()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Core.Ensure.IsNotOfType<object>( param ) );
        }

        [Fact]
        public void IsAssignableToType_ShouldPass_WhenParamIsAssignableToType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Core.Ensure.IsAssignableToType<object>( param ) );
        }

        [Fact]
        public void IsAssignableToType_ShouldPass_WhenParamIsOfExactType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Core.Ensure.IsAssignableToType<T>( param ) );
        }

        [Fact]
        public void IsAssignableToType_ShouldThrow_WhenParamIsNotAssignableToType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Core.Ensure.IsAssignableToType<IEnumerable<T>>( param ) );
        }

        [Fact]
        public void IsNotAssignableToType_ShouldPass_WhenParamIsNotAssignableToType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Core.Ensure.IsNotAssignableToType<IEnumerable<T>>( param ) );
        }

        [Fact]
        public void IsNotAssignableToType_ShouldThrow_WhenParamIsOfExactType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Core.Ensure.IsNotAssignableToType<T>( param ) );
        }

        [Fact]
        public void IsNotAssignableToType_ShouldThrow_WhenParamIsAssignableToType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Core.Ensure.IsNotAssignableToType<object>( param ) );
        }

        [Fact]
        public void Equals_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.Equals( param, param, EqualityComparer ) );
        }

        [Fact]
        public void Equals_ShouldThrow_WhenParamIsNotEqualToValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldThrow( () => Core.Ensure.Equals( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotEquals_ShouldPass_WhenParamIsNotEqualToValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.NotEquals( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotEquals_ShouldThrow_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow( () => Core.Ensure.NotEquals( param, param, EqualityComparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldPass_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsGreaterThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsGreaterThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsGreaterThan( param, param, Comparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsGreaterThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsGreaterThanOrEqualTo( param, param, Comparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldThrow_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsGreaterThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldPass_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsLessThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsLessThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsLessThan( param, param, Comparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsLessThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsLessThanOrEqualTo( param, param, Comparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldThrow_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsLessThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsInRange( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsInRange( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsInRange( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldThrow_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldThrow_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldThrow_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldPass_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsNotInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldPass_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsNotInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldPass_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsNotInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrow_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrow_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInRange( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrow_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInRange( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrow_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInRange( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldPass_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrow_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsNotInExclusiveRange( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldThrow_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Core.Ensure.IsNotInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsEmpty_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Core.Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldPass_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldPass( () => Core.Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrow_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrow( () => Core.Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrow_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldThrow( () => Core.Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Core.Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldPass( () => Core.Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrow( () => Core.Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldThrow( () => Core.Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenEnumerableIsNull()
        {
            var param = Fixture.CreateDefault<IEnumerable<T>>();
            ShouldPass( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenCollectionIsNull()
        {
            var param = Fixture.CreateDefault<T[]>();
            ShouldPass( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldPass( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrow_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrow( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrow_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldThrow( () => Core.Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldPass( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenEnumerableIsNull()
        {
            var param = Fixture.CreateDefault<IEnumerable<T>>();
            ShouldThrow( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrow( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenCollectionIsNull()
        {
            var param = Fixture.CreateDefault<T[]>();
            ShouldThrow( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldThrow( () => Core.Ensure.IsNotNullOrEmpty( param ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsAtLeastPassData ) )]
        public void ContainsAtLeast_ShouldPass_WhenCollectionContainsEnoughElements(int minCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldPass( () => Core.Ensure.ContainsAtLeast( param, minCount ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsAtLeastThrowData ) )]
        public void ContainsAtLeast_ShouldThrow_WhenCollectionContainsTooFewElements(int minCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldThrow( () => Core.Ensure.ContainsAtLeast( param, minCount ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsAtMostPassData ) )]
        public void ContainsAtMost_ShouldPass_WhenCollectionDoesntContainTooManyElements(int maxCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldPass( () => Core.Ensure.ContainsAtMost( param, maxCount ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsAtMostThrowData ) )]
        public void ContainsAtMost_ShouldThrow_WhenCollectionContainsTooManyElements(int maxCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldThrow( () => Core.Ensure.ContainsAtMost( param, maxCount ) );
        }

        [Fact]
        public void ContainsExactly_ShouldPass_WhenCollectionContainsExactlyTheRightAmountOfElements()
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldPass( () => Core.Ensure.ContainsExactly( param, 3 ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsExactlyThrowData ) )]
        public void ContainsExactly_ShouldThrow_WhenCollectionContainsTooFewOrManyElements(int count)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldThrow( () => Core.Ensure.ContainsExactly( param, count ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsInRangePassData ) )]
        public void ContainsInRange_ShouldPass_WhenCollectionContainsCorrectAmountOfElements(int minCount, int maxCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldPass( () => Core.Ensure.ContainsInRange( param, minCount, maxCount ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsInRangeThrowData ) )]
        public void ContainsInRange_ShouldThrow_WhenCollectionContainsTooManyOrTooFewElements(int minCount, int maxCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldThrow( () => Core.Ensure.ContainsInRange( param, minCount, maxCount ) );
        }

        [Fact]
        public void Contains_ShouldPass_WhenEnumerableContainsElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Core.Ensure.Contains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void Contains_ShouldThrow_WhenEnumerableDoesntContainElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrow( () => Core.Ensure.Contains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotContains_ShouldPass_WhenEnumerableDoesntContainElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldPass( () => Core.Ensure.NotContains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotContains_ShouldThrow_WhenEnumerableContainsElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldThrow( () => Core.Ensure.NotContains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void ForAny_ShouldPass_WhenAtLeastOneElementPassesThePredicate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Core.Ensure.ForAny( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenNoElementsPassThePredicate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrow( () => Core.Ensure.ForAny( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrow( () => Core.Ensure.ForAny( param, _ => true ) );
        }

        [Fact]
        public void ForAny_ShouldPass_WhenAtLeastOneElementPassesThePredicate_WithDelegate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Core.Ensure.ForAny( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenNoElementsPassThePredicate_WithDelegate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrow( () => Core.Ensure.ForAny( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenEnumerableIsEmpty_WithDelegate()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrow( () => Core.Ensure.ForAny( param, _ => true, () => string.Empty ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenAllElementsPassThePredicate()
        {
            var value = Fixture.Create<T>();
            var param = Enumerable.Range( 0, 3 ).Select( _ => value );
            ShouldPass( () => Core.Ensure.ForAll( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Core.Ensure.ForAll( param, _ => false ) );
        }

        [Fact]
        public void ForAll_ShouldThrow_WhenAtLeastOneElementFailsThePredicate()
        {
            var (value, other) = Fixture.CreateDistinctCollection<T>( 2 );
            var param = Enumerable.Range( 0, 3 ).Select( _ => value ).Append( other );
            ShouldThrow( () => Core.Ensure.ForAll( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenAllElementsPassThePredicate_WithDelegate()
        {
            var value = Fixture.Create<T>();
            var param = Enumerable.Range( 0, 3 ).Select( _ => value );
            ShouldPass( () => Core.Ensure.ForAll( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenEnumerableIsEmpty_WithDelegate()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Core.Ensure.ForAll( param, _ => false, () => string.Empty ) );
        }

        [Fact]
        public void ForAll_ShouldThrow_WhenAtLeastOneElementFailsThePredicate_WithDelegate()
        {
            var (value, other) = Fixture.CreateDistinctCollection<T>( 2 );
            var param = Enumerable.Range( 0, 3 ).Select( _ => value ).Append( other );
            ShouldThrow( () => Core.Ensure.ForAll( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        protected void IsNull_ShouldPass_WhenParamIsNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldPass( () => Core.Ensure.IsNull( param, EqualityComparer ) );
        }

        protected void IsNull_ShouldThrow_WhenParamIsNotNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow( () => Core.Ensure.IsNull( param, EqualityComparer ) );
        }

        protected void IsNotNull_ShouldPass_WhenParamIsNotNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsNotNull( param, EqualityComparer ) );
        }

        protected void IsNotNull_ShouldThrow_WhenParamIsNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldThrow<ArgumentNullException>( () => Core.Ensure.IsNotNull( param, EqualityComparer ) );
        }
    }
}
