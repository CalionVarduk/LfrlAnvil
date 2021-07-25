using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Assert
{
    public abstract class AssertTests<T> : AssertTestsBase
    {
        protected readonly IEqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;
        protected readonly IComparer<T> Comparer = Comparer<T>.Default;

        [Fact]
        public void IsDefault_ShouldPass_WhenParamIsDefault()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldPass( () => Common.Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsDefault_ShouldThrow_WhenParamIsNotDefault()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow( () => Common.Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldPass_WhenParamIsNotDefault()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Common.Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldThrow_WhenParamIsDefault()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldThrow( () => Common.Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsOfType_ShouldThrow_WhenTypesDontMatch()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Common.Assert.IsOfType<object>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldPass_WhenTypesDontMatch()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Common.Assert.IsNotOfType<object>( param ) );
        }

        [Fact]
        public void IsAssignableToType_ShouldPass_WhenParamIsAssignableToType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Common.Assert.IsAssignableToType<object>( param ) );
        }

        [Fact]
        public void IsAssignableToType_ShouldPass_WhenParamIsOfExactType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Common.Assert.IsAssignableToType<T>( param ) );
        }

        [Fact]
        public void IsAssignableToType_ShouldThrow_WhenParamIsNotAssignableToType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Common.Assert.IsAssignableToType<IEnumerable<T>>( param ) );
        }

        [Fact]
        public void IsNotAssignableToType_ShouldPass_WhenParamIsNotAssignableToType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Common.Assert.IsNotAssignableToType<IEnumerable<T>>( param ) );
        }

        [Fact]
        public void IsNotAssignableToType_ShouldThrow_WhenParamIsOfExactType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Common.Assert.IsNotAssignableToType<T>( param ) );
        }

        [Fact]
        public void IsNotAssignableToType_ShouldThrow_WhenParamIsAssignableToType()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Common.Assert.IsNotAssignableToType<object>( param ) );
        }

        [Fact]
        public void Equals_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Common.Assert.Equals( param, param, EqualityComparer ) );
        }

        [Fact]
        public void Equals_ShouldThrow_WhenParamIsNotEqualToValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldThrow( () => Common.Assert.Equals( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotEquals_ShouldPass_WhenParamIsNotEqualToValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldPass( () => Common.Assert.NotEquals( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotEquals_ShouldThrow_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow( () => Common.Assert.NotEquals( param, param, EqualityComparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldPass_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsGreaterThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsGreaterThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsGreaterThan( param, param, Comparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsGreaterThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Common.Assert.IsGreaterThanOrEqualTo( param, param, Comparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldThrow_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsGreaterThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldPass_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsLessThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsLessThan( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsLessThan( param, param, Comparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsLessThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Common.Assert.IsLessThanOrEqualTo( param, param, Comparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldThrow_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsLessThanOrEqualTo( param, value, Comparer ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Common.Assert.IsBetween( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsBetween( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsBetween( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Common.Assert.IsBetween( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsBetween_ShouldThrow_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsBetween( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsBetween_ShouldThrow_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsBetween( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsBetween_ShouldThrow_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsBetween( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotBetween_ShouldPass_WhenParamIsLessThanMinValue_WithExplicitComparer()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Common.Assert.IsNotBetween( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotBetween_ShouldPass_WhenParamIsGreaterThanMaxValue_WithExplicitComparer()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Common.Assert.IsNotBetween( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotBetween_ShouldPass_WhenMinIsGreaterThanMax_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsNotBetween( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsBetweenTwoDistinctValues_WithExplicitComparer()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsNotBetween( param, min, max, Comparer ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsEqualToMinValue_WithExplicitComparer()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsNotBetween( param, param, max, Comparer ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsEqualToMaxValue_WithExplicitComparer()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsNotBetween( param, min, param, Comparer ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsEqualToMinAndMaxValues_WithExplicitComparer()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow<ArgumentOutOfRangeException>( () => Common.Assert.IsNotBetween( param, param, param, Comparer ) );
        }

        [Fact]
        public void IsEmpty_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Common.Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldPass_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldPass( () => Common.Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrow_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrow( () => Common.Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrow_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldThrow( () => Common.Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Common.Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldPass( () => Common.Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrow( () => Common.Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldThrow( () => Common.Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenEnumerableIsNull()
        {
            var param = Fixture.CreateDefault<IEnumerable<T>>();
            ShouldPass( () => Common.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Common.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenCollectionIsNull()
        {
            var param = Fixture.CreateDefault<T[]>();
            ShouldPass( () => Common.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldPass( () => Common.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrow_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrow( () => Common.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrow_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldThrow( () => Common.Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenEnumerableIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Common.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenCollectionIsNotEmpty()
        {
            var param = Fixture.CreateMany<T>().ToList();
            ShouldPass( () => Common.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenEnumerableIsNull()
        {
            var param = Fixture.CreateDefault<IEnumerable<T>>();
            ShouldThrow( () => Common.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrow( () => Common.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenCollectionIsNull()
        {
            var param = Fixture.CreateDefault<T[]>();
            ShouldThrow( () => Common.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<T>();
            ShouldThrow( () => Common.Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void Contains_ShouldPass_WhenEnumerableContainsElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Common.Assert.Contains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void Contains_ShouldThrow_WhenEnumerableDoesntContainElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrow( () => Common.Assert.Contains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotContains_ShouldPass_WhenEnumerableDoesntContainElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldPass( () => Common.Assert.NotContains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void NotContains_ShouldThrow_WhenEnumerableContainsElement_WithExplicitComparer()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldThrow( () => Common.Assert.NotContains( param, value, EqualityComparer ) );
        }

        [Fact]
        public void ForAny_ShouldPass_WhenAtLeastOneElementPassesThePredicate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Common.Assert.ForAny( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenNoElementsPassThePredicate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrow( () => Common.Assert.ForAny( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrow( () => Common.Assert.ForAny( param, _ => true ) );
        }

        [Fact]
        public void ForAny_ShouldPass_WhenAtLeastOneElementPassesThePredicate_WithDelegate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Common.Assert.ForAny( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenNoElementsPassThePredicate_WithDelegate()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrow( () => Common.Assert.ForAny( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenEnumerableIsEmpty_WithDelegate()
        {
            var param = Enumerable.Empty<T>();
            ShouldThrow( () => Common.Assert.ForAny( param, _ => true, () => string.Empty ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenAllElementsPassThePredicate()
        {
            var value = Fixture.Create<T>();
            var param = Enumerable.Range( 0, 3 ).Select( _ => value );
            ShouldPass( () => Common.Assert.ForAll( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Common.Assert.ForAll( param, _ => false ) );
        }

        [Fact]
        public void ForAll_ShouldThrow_WhenAtLeastOneElementFailsThePredicate()
        {
            var (value, other) = Fixture.CreateDistinctCollection<T>( 2 );
            var param = Enumerable.Range( 0, 3 ).Select( _ => value ).Append( other );
            ShouldThrow( () => Common.Assert.ForAll( param, e => EqualityComparer.Equals( e, value ) ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenAllElementsPassThePredicate_WithDelegate()
        {
            var value = Fixture.Create<T>();
            var param = Enumerable.Range( 0, 3 ).Select( _ => value );
            ShouldPass( () => Common.Assert.ForAll( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenEnumerableIsEmpty_WithDelegate()
        {
            var param = Enumerable.Empty<T>();
            ShouldPass( () => Common.Assert.ForAll( param, _ => false, () => string.Empty ) );
        }

        [Fact]
        public void ForAll_ShouldThrow_WhenAtLeastOneElementFailsThePredicate_WithDelegate()
        {
            var (value, other) = Fixture.CreateDistinctCollection<T>( 2 );
            var param = Enumerable.Range( 0, 3 ).Select( _ => value ).Append( other );
            ShouldThrow( () => Common.Assert.ForAll( param, e => EqualityComparer.Equals( e, value ), () => string.Empty ) );
        }

        protected void IsNull_ShouldPass_WhenParamIsNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldPass( () => Common.Assert.IsNull( param, EqualityComparer ) );
        }

        protected void IsNull_ShouldThrow_WhenParamIsNotNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow( () => Common.Assert.IsNull( param, EqualityComparer ) );
        }

        protected void IsNotNull_ShouldPass_WhenParamIsNotNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Common.Assert.IsNotNull( param, EqualityComparer ) );
        }

        protected void IsNotNull_ShouldThrow_WhenParamIsNull_WithExplicitComparer_Impl()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldThrow<ArgumentNullException>( () => Common.Assert.IsNotNull( param, EqualityComparer ) );
        }
    }
}
