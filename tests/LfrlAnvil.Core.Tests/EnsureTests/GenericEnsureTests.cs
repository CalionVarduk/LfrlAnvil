using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using Xunit;

namespace LfrlAnvil.Tests.EnsureTests
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
            ShouldPass( () => Ensure.IsDefault( param ) );
        }

        [Fact]
        public void IsDefault_ShouldThrowArgumentException_WhenParamIsNotDefault()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrowArgumentException( () => Ensure.IsDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldPass_WhenParamIsNotDefault()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Ensure.IsNotDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldThrowArgumentException_WhenParamIsDefault()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldThrowArgumentException( () => Ensure.IsNotDefault( param ) );
        }

        [Fact]
        public void IsOfType_ShouldThrowArgumentException_WhenTypesDontMatch()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrowArgumentException( () => Ensure.IsOfType<object>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldPass_WhenTypesDontMatch()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Ensure.IsNotOfType<object>( param ) );
        }

        [Fact]
        public void IsInstanceOfType_ShouldPass_WhenParamIsInstanceOfType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Ensure.IsInstanceOfType<object>( param ) );
        }

        [Fact]
        public void IsInstanceOfType_ShouldPass_WhenParamIsOfExactType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Ensure.IsInstanceOfType<T>( param ) );
        }

        [Fact]
        public void IsInstanceOfType_ShouldThrowArgumentException_WhenParamIsNotInstanceOfType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrowArgumentException( () => Ensure.IsInstanceOfType<IEnumerable<T>>( param ) );
        }

        [Fact]
        public void IsNotInstanceOfType_ShouldPass_WhenParamIsNotInstanceOfType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Ensure.IsNotInstanceOfType<IEnumerable<T>>( param ) );
        }

        [Fact]
        public void IsNotInstanceOfType_ShouldThrowArgumentException_WhenParamIsOfExactType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrowArgumentException( () => Ensure.IsNotInstanceOfType<T>( param ) );
        }

        [Fact]
        public void IsNotInstanceOfType_ShouldThrowArgumentException_WhenParamIsInstanceOfType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrowArgumentException( () => Ensure.IsNotInstanceOfType<object>( param ) );
        }

        [Fact]
        public void Equals_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Ensure.Equals( param, param, EqualityComparer ) );
        }

        [Fact]
        public void Equals_ShouldThrowArgumentException_WhenParamIsNotEqualToValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldThrowArgumentException( () => Ensure.Equals( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotEquals_ShouldPass_WhenParamIsNotEqualToValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldPass( () => Ensure.NotEquals( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotEquals_ShouldThrowArgumentException_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrowArgumentException( () => Ensure.NotEquals( param, param, EqualityComparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldPass_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsGreaterThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrowArgumentOutOfRangeException_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsGreaterThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsGreaterThan( param, param, Comparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsGreaterThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Ensure.IsGreaterThanOrEqualTo( param, param, Comparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldThrowArgumentOutOfRangeException_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsGreaterThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldPass_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsLessThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrowArgumentOutOfRangeException_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsLessThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsLessThan( param, param, Comparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsLessThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Ensure.IsLessThanOrEqualTo( param, param, Comparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldThrowArgumentOutOfRangeException_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsLessThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Ensure.IsInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsInRange( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsInRange( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldPass_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Ensure.IsInRange( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInRange_ShouldThrowArgumentOutOfRangeException_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldPass_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Ensure.IsNotInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldPass_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Ensure.IsNotInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldPass_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsNotInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInRange( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInRange( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsNotInRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInRange( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldPass_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Ensure.IsInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Ensure.IsNotInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Ensure.IsNotInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsNotInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsNotInExclusiveRange( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Ensure.IsNotInExclusiveRange( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Ensure.IsNotInExclusiveRange( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsNotInExclusiveRange_ShouldThrowArgumentOutOfRangeException_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrowExactly<ArgumentOutOfRangeException>( () => Ensure.IsNotInExclusiveRange( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsEmpty_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldPass_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldPass( () => Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrowArgumentException_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrowArgumentException( () => Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrowArgumentException_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldThrowArgumentException( () => Ensure.IsEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldPass( () => Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrowArgumentException_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrowArgumentException( () => Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrowArgumentException_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldThrowArgumentException( () => Ensure.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenEnumerableIsNull()
        {
            var param = Fixture.CreateDefault<IEnumerable<T>>();
            ShouldPass( () => Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenCollectionIsNull()
        {
            var param = Fixture.CreateDefault<T[]>();
            ShouldPass( () => Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldPass( () => Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrowArgumentException_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrowArgumentException( () => Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrowArgumentException_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldThrowArgumentException( () => Ensure.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldPass( () => Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrowArgumentException_WhenEnumerableIsNull()
        {
            var param = Fixture.CreateDefault<IEnumerable<T>>();
            ShouldThrowArgumentException( () => Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrowArgumentException_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrowArgumentException( () => Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrowArgumentException_WhenCollectionIsNull()
        {
            var param = Fixture.CreateDefault<T[]>();
            ShouldThrowArgumentException( () => Ensure.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrowArgumentException_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldThrowArgumentException( () => Ensure.IsNotNullOrEmpty( param ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsAtLeastPassData ) )]
        public void ContainsAtLeast_ShouldPass_WhenCollectionContainsEnoughElements(int minCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldPass( () => Ensure.ContainsAtLeast( param, minCount ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsAtLeastThrowData ) )]
        public void ContainsAtLeast_ShouldThrowArgumentException_WhenCollectionContainsTooFewElements(int minCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldThrowArgumentException( () => Ensure.ContainsAtLeast( param, minCount ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsAtMostPassData ) )]
        public void ContainsAtMost_ShouldPass_WhenCollectionDoesntContainTooManyElements(int maxCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldPass( () => Ensure.ContainsAtMost( param, maxCount ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsAtMostThrowData ) )]
        public void ContainsAtMost_ShouldThrowArgumentException_WhenCollectionContainsTooManyElements(int maxCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldThrowArgumentException( () => Ensure.ContainsAtMost( param, maxCount ) );
        }

        [Fact]
        public void ContainsExactly_ShouldPass_WhenCollectionContainsExactlyTheRightAmountOfElements()
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldPass( () => Ensure.ContainsExactly( param, 3 ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsExactlyThrowData ) )]
        public void ContainsExactly_ShouldThrowArgumentException_WhenCollectionContainsTooFewOrManyElements(int count)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldThrowArgumentException( () => Ensure.ContainsExactly( param, count ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsInRangePassData ) )]
        public void ContainsInRange_ShouldPass_WhenCollectionContainsCorrectAmountOfElements(int minCount, int maxCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldPass( () => Ensure.ContainsInRange( param, minCount, maxCount ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetContainsInRangeThrowData ) )]
        public void ContainsInRange_ShouldThrowArgumentException_WhenCollectionContainsTooManyOrTooFewElements(int minCount, int maxCount)
        {
            var param = Fixture.CreateMany<T>( 3 );
            ShouldThrowArgumentException( () => Ensure.ContainsInRange( param, minCount, maxCount ) );
        }

        [Fact]
        public void Contains_ShouldPass_WhenEnumerableContainsElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Ensure.Contains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void Contains_ShouldThrowArgumentException_WhenEnumerableDoesntContainElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrowArgumentException( () => Ensure.Contains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotContains_ShouldPass_WhenEnumerableDoesntContainElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldPass( () => Ensure.NotContains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotContains_ShouldThrowArgumentException_WhenEnumerableContainsElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldThrowArgumentException( () => Ensure.NotContains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void ForAny_ShouldPass_WhenAtLeastOneElementPassesThePredicate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Ensure.ForAny( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAny_ShouldThrowArgumentException_WhenNoElementsPassThePredicate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrowArgumentException( () => Ensure.ForAny( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAny_ShouldThrowArgumentException_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrowArgumentException( () => Ensure.ForAny( param, _ => true ) );
        }

        [Fact]
        public void ForAny_ShouldPass_WhenAtLeastOneElementPassesThePredicate_WithDelegate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Ensure.ForAny( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void ForAny_ShouldThrowArgumentException_WhenNoElementsPassThePredicate_WithDelegate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrowArgumentException( () => Ensure.ForAny( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void ForAny_ShouldThrowArgumentException_WhenEnumerableIsEmpty_WithDelegate()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrowArgumentException( () => Ensure.ForAny( param, _ => true, () => string.Empty ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenAllElementsPassThePredicate()
        {
            var value = Fixture.Create<T>();
            var param = Enumerable.Range( 0, 3 ).Select( _ => value );
            ShouldPass( () => Ensure.ForAll( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Ensure.ForAll( param, _ => false ) );
        }

        [Fact]
        public void ForAll_ShouldThrowArgumentException_WhenAtLeastOneElementFailsThePredicate()
        {
            var (value, other) = Fixture.CreateDistinctCollection<T>( 2 );
            var param = Enumerable.Range( 0, 3 ).Select( _ => value ).Append( other );
            ShouldThrowArgumentException( () => Ensure.ForAll( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenAllElementsPassThePredicate_WithDelegate()
        {
            var value = Fixture.Create<T>();
            var param = Enumerable.Range( 0, 3 ).Select( _ => value );
            ShouldPass( () => Ensure.ForAll( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenEnumerableIsEmpty_WithDelegate()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Ensure.ForAll( param, _ => false, () => string.Empty ) );
        }

        [Fact]
        public void ForAll_ShouldThrowArgumentException_WhenAtLeastOneElementFailsThePredicate_WithDelegate()
        {
            var (value, other) = Fixture.CreateDistinctCollection<T>( 2 );
            var param = Enumerable.Range( 0, 3 ).Select( _ => value ).Append( other );
            ShouldThrowArgumentException( () => Ensure.ForAll( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void IsOrdered_ShouldPass_ForEmptyCollection_WithExplicitComparer()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Ensure.IsOrdered( param, Comparer ) );
        }

        [Fact]
        public void IsOrdered_ShouldPass_ForCollectionWithOneElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T>( 1 );
            ShouldPass( () => Ensure.IsOrdered( param, Comparer ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetIsOrderedPassData ) )]
        public void IsOrdered_ShouldPass_ForOrderedCollection_WithExplicitComparer(IEnumerable<T> param)
        {
            ShouldPass( () => Ensure.IsOrdered( param, Comparer ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnsureTestsData<T>.GetIsOrderedThrowData ) )]
        public void IsOrdered_ShouldThrowArgumentException_ForUnorderedCollection_WithExplicitComparer(IEnumerable<T> param)
        {
            ShouldThrowArgumentException( () => Ensure.IsOrdered( param, Comparer ) );
        }

        protected void IsNull_ShouldPass_WhenParamIsNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldPass( () => Ensure.IsNull( param, EqualityComparer ) );
        }

        protected void IsNull_ShouldThrowArgumentException_WhenParamIsNotNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrowArgumentException( () => Ensure.IsNull( param, EqualityComparer ) );
        }

        protected void IsNotNull_ShouldPass_WhenParamIsNotNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Ensure.IsNotNull( param, EqualityComparer ) );
        }

        protected void IsNotNull_ShouldThrowArgumentNullException_WhenParamIsNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldThrowExactly<ArgumentNullException>( () => Ensure.IsNotNull( param, EqualityComparer ) );
        }
    }
}
