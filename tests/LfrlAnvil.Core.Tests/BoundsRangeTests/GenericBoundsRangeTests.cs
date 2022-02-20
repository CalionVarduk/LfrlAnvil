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
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Tests.BoundsRangeTests
{
    [GenericTestClass( typeof( GenericBoundsRangeTestsData<> ) )]
    public abstract class GenericBoundsRangeTests<T> : TestsBase
        where T : IComparable<T>
    {
        [Fact]
        public void Empty_ShouldReturnCorrectResult()
        {
            var result = BoundsRange<T>.Empty;
            result.Count.Should().Be( 0 );
        }

        [Fact]
        public void Default_ShouldReturnCorrectResult()
        {
            var sut = default( BoundsRange<T> );
            sut.Count.Should().Be( 0 );
        }

        [Fact]
        public void Ctor_WithBounds_ShouldReturnCorrectResult()
        {
            Factory_WithBounds_ShouldReturnCorrectResult( b => new BoundsRange<T>( b ) );
        }

        [Fact]
        public void Ctor_WithBounds_ShouldReturnCorrectResult_WithTheSameMinAndMax()
        {
            Factory_WithBounds_ShouldReturnCorrectResult_WithTheSameMinAndMax( b => new BoundsRange<T>( b ) );
        }

        [Fact]
        public void Ctor_WithRange_ShouldReturnCorrectResult_WhenRangeIsEmpty()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenRangeIsEmpty( r => new BoundsRange<T>( r ) );
        }

        [Fact]
        public void Ctor_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElement()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElement( r => new BoundsRange<T>( r ) );
        }

        [Fact]
        public void Ctor_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElementWithTheSameMinAndMax()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElementWithTheSameMinAndMax( r => new BoundsRange<T>( r ) );
        }

        [Fact]
        public void Ctor_WithRange_ShouldReturnCorrectResult_WhenAllInRangeHaveTheSameMinAndMax()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenAllInRangeHaveTheSameMinAndMax( r => new BoundsRange<T>( r ) );
        }

        [Fact]
        public void Ctor_WithRange_ShouldReturnCorrectResult_WhenAllValuesAreUnique()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenAllValuesAreUnique( r => new BoundsRange<T>( r ) );
        }

        [Fact]
        public void Ctor_WithRange_ShouldReturnCorrectResult_WhenSomeValuesAreRedundant()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenSomeValuesAreRedundant( r => new BoundsRange<T>( r ) );
        }

        [Fact]
        public void Ctor_WithRange_ShouldThrowArgumentException_WhenBoundsAreNotOrdered()
        {
            Factory_WithRange_ShouldThrowArgumentException_WhenBoundsAreNotOrdered( r => new BoundsRange<T>( r ) );
        }

        [Fact]
        public void Create_WithBounds_ShouldReturnCorrectResult()
        {
            Factory_WithBounds_ShouldReturnCorrectResult( BoundsRange.Create );
        }

        [Fact]
        public void Create_WithBounds_ShouldReturnCorrectResult_WithTheSameMinAndMax()
        {
            Factory_WithBounds_ShouldReturnCorrectResult_WithTheSameMinAndMax( BoundsRange.Create );
        }

        [Fact]
        public void Create_WithRange_ShouldReturnCorrectResult_WhenRangeIsEmpty()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenRangeIsEmpty( BoundsRange.Create );
        }

        [Fact]
        public void Create_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElement()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElement( BoundsRange.Create );
        }

        [Fact]
        public void Create_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElementWithTheSameMinAndMax()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElementWithTheSameMinAndMax( BoundsRange.Create );
        }

        [Fact]
        public void Create_WithRange_ShouldReturnCorrectResult_WhenAllInRangeHaveTheSameMinAndMax()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenAllInRangeHaveTheSameMinAndMax( BoundsRange.Create );
        }

        [Fact]
        public void Create_WithRange_ShouldReturnCorrectResult_WhenAllValuesAreUnique()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenAllValuesAreUnique( BoundsRange.Create );
        }

        [Fact]
        public void Create_WithRange_ShouldReturnCorrectResult_WhenSomeValuesAreRedundant()
        {
            Factory_WithRange_ShouldReturnCorrectResult_WhenSomeValuesAreRedundant( BoundsRange.Create );
        }

        [Fact]
        public void Create_WithRange_ShouldThrowArgumentException_WhenBoundsAreNotOrdered()
        {
            Factory_WithRange_ShouldThrowArgumentException_WhenBoundsAreNotOrdered( BoundsRange.Create );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetToStringData ) )]
        public void ToString_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range, string expected)
        {
            var sut = BoundsRange.Create( range );
            var result = sut.ToString();
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetGetHashCodeData ) )]
        public void GetHashCode_ShouldReturnMixOfAllContainedBounds(IEnumerable<Bounds<T>> range)
        {
            var sut = BoundsRange.Create( range );
            var expected = Hash.Default.AddRange( sut.SelectMany( r => r.AsEnumerable() ) ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetEqualsData ) )]
        public void Equals_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range1, IEnumerable<Bounds<T>> range2, bool expected)
        {
            var sut = BoundsRange.Create( range1 );
            var other = BoundsRange.Create( range2 );

            var result = sut.Equals( other );

            result.Should().Be( expected );
        }

        [Fact]
        public void Flatten_ShouldReturnNull_WhenRangeIsEmpty()
        {
            var sut = BoundsRange<T>.Empty;
            var result = sut.Flatten();
            result.Should().BeNull();
        }

        [Fact]
        public void Flatten_ShouldReturnCorrectResult_WhenRangeIsNotEmpty()
        {
            var (a, b, c, d) = Fixture.CreateDistinctSortedCollection<T>( 4 );
            var bounds1 = Bounds.Create( a, b );
            var bounds2 = Bounds.Create( c, d );
            var sut = new BoundsRange<T>( new[] { bounds1, bounds2 } );

            var result = sut.Flatten();

            result.Should().Be( Bounds.Create( a, d ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetFindBoundsIndexData ) )]
        public void FindBoundsIndex_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range, T value, int expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.FindBoundsIndex( value );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetFindBoundsData ) )]
        public void FindBounds_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range, T value, Bounds<T>? expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.FindBounds( value );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetContainsWithSingleValueData ) )]
        public void Contains_WithSingleValue_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range, T value, bool expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.Contains( value );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetContainsWithBoundsData ) )]
        public void Contains_WithBounds_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range, Bounds<T> value, bool expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.Contains( value );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetContainsData ) )]
        public void Contains_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range, IEnumerable<Bounds<T>> otherRange, bool expected)
        {
            var sut = new BoundsRange<T>( range );
            var other = new BoundsRange<T>( otherRange );
            var result = sut.Contains( other );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetIntersectsWithBoundsData ) )]
        public void Intersects_WithBounds_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range, Bounds<T> value, bool expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.Intersects( value );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetIntersectsData ) )]
        public void Intersects_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range, IEnumerable<Bounds<T>> otherRange, bool expected)
        {
            var sut = new BoundsRange<T>( range );
            var other = new BoundsRange<T>( otherRange );
            var result = sut.Intersects( other );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetGetIntersectionWithBoundsData ) )]
        public void GetIntersection_WithBounds_ShouldReturnCorrectResult(
            IEnumerable<Bounds<T>> range,
            Bounds<T> value,
            IEnumerable<Bounds<T>> expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.GetIntersection( value );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetGetIntersectionData ) )]
        public void GetIntersection_ShouldReturnCorrectResult(
            IEnumerable<Bounds<T>> range,
            IEnumerable<Bounds<T>> otherRange,
            IEnumerable<Bounds<T>> expected)
        {
            var sut = new BoundsRange<T>( range );
            var other = new BoundsRange<T>( otherRange );
            var result = sut.GetIntersection( other );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetMergeWithWithBoundsData ) )]
        public void MergeWith_WithBounds_ShouldReturnCorrectResult(
            IEnumerable<Bounds<T>> range,
            Bounds<T> value,
            IEnumerable<Bounds<T>> expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.MergeWith( value );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetMergeWithData ) )]
        public void MergeWith_ShouldReturnCorrectResult(
            IEnumerable<Bounds<T>> range,
            IEnumerable<Bounds<T>> otherRange,
            IEnumerable<Bounds<T>> expected)
        {
            var sut = new BoundsRange<T>( range );
            var other = new BoundsRange<T>( otherRange );
            var result = sut.MergeWith( other );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetRemoveWithBoundsData ) )]
        public void Remove_WithBounds_ShouldReturnCorrectResult(
            IEnumerable<Bounds<T>> range,
            Bounds<T> value,
            IEnumerable<Bounds<T>> expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.Remove( value );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetRemoveData ) )]
        public void Remove_ShouldReturnCorrectResult(
            IEnumerable<Bounds<T>> range,
            IEnumerable<Bounds<T>> otherRange,
            IEnumerable<Bounds<T>> expected)
        {
            var sut = new BoundsRange<T>( range );
            var other = new BoundsRange<T>( otherRange );
            var result = sut.Remove( other );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetComplementData ) )]
        public void Complement_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range, IEnumerable<Bounds<T>> expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.Complement();
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetComplementWithBoundsData ) )]
        public void Complement_WithBounds_ShouldReturnCorrectResult(
            IEnumerable<Bounds<T>> range,
            Bounds<T> container,
            IEnumerable<Bounds<T>> expected)
        {
            var sut = new BoundsRange<T>( range );
            var result = sut.Complement( container );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetComplementWithRangeData ) )]
        public void Complement_WithRange_ShouldReturnCorrectResult(
            IEnumerable<Bounds<T>> range,
            IEnumerable<Bounds<T>> container,
            IEnumerable<Bounds<T>> expected)
        {
            var sut = new BoundsRange<T>( range );
            var other = new BoundsRange<T>( container );
            var result = sut.Complement( other );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void Normalize_ShouldReturnTarget_WhenTargetIsEmpty()
        {
            var sut = BoundsRange<T>.Empty;
            var result = sut.Normalize( (a, b) => a.Equals( b ) );
            result.Should().BeEmpty();
        }

        [Fact]
        public void Normalize_ShouldReturnTarget_WhenTargetContainsOnlyOneItem()
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<T>( count: 2 );
            var item = Bounds.Create( min, max );
            var sut = new BoundsRange<T>( item );

            var result = sut.Normalize( (a, b) => a.Equals( b ) );

            result.Should().BeSequentiallyEqualTo( item );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetEqualsData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(IEnumerable<Bounds<T>> range1, IEnumerable<Bounds<T>> range2, bool expected)
        {
            var sut = BoundsRange.Create( range1 );
            var other = BoundsRange.Create( range2 );

            var result = sut == other;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBoundsRangeTestsData<T>.GetNotEqualsData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(
            IEnumerable<Bounds<T>> range1,
            IEnumerable<Bounds<T>> range2,
            bool expected)
        {
            var sut = BoundsRange.Create( range1 );
            var other = BoundsRange.Create( range2 );

            var result = sut != other;

            result.Should().Be( expected );
        }

        private void Factory_WithBounds_ShouldReturnCorrectResult(Func<Bounds<T>, BoundsRange<T>> factory)
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            var bounds = Bounds.Create( min, max );

            var result = factory( bounds );

            using ( new AssertionScope() )
            {
                result.Should().BeSequentiallyEqualTo( bounds );
                result[0].Should().Be( bounds );
            }
        }

        private void Factory_WithBounds_ShouldReturnCorrectResult_WithTheSameMinAndMax(Func<Bounds<T>, BoundsRange<T>> factory)
        {
            var value = Fixture.Create<T>();
            var bounds = Bounds.Create( value, value );

            var result = factory( bounds );

            using ( new AssertionScope() )
            {
                result.Should().BeSequentiallyEqualTo( bounds );
                result[0].Should().Be( bounds );
            }
        }

        private void Factory_WithRange_ShouldReturnCorrectResult_WhenRangeIsEmpty(Func<IEnumerable<Bounds<T>>, BoundsRange<T>> factory)
        {
            var range = Enumerable.Empty<Bounds<T>>();
            var result = factory( range );
            result.Count.Should().Be( 0 );
        }

        private void Factory_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElement(
            Func<IEnumerable<Bounds<T>>, BoundsRange<T>> factory)
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            var bounds = Bounds.Create( min, max );
            var range = new[] { bounds };

            var result = factory( range );

            using ( new AssertionScope() )
            {
                result.Should().BeSequentiallyEqualTo( bounds );
                result[0].Should().Be( bounds );
            }
        }

        private void Factory_WithRange_ShouldReturnCorrectResult_WhenRangeHasOneElementWithTheSameMinAndMax(
            Func<IEnumerable<Bounds<T>>, BoundsRange<T>> factory)
        {
            var value = Fixture.Create<T>();
            var bounds = Bounds.Create( value, value );
            var range = new[] { bounds };

            var result = factory( range );

            using ( new AssertionScope() )
            {
                result.Should().BeSequentiallyEqualTo( bounds );
                result[0].Should().Be( bounds );
            }
        }

        private void Factory_WithRange_ShouldReturnCorrectResult_WhenAllInRangeHaveTheSameMinAndMax(
            Func<IEnumerable<Bounds<T>>, BoundsRange<T>> factory)
        {
            var value = Fixture.Create<T>();
            var bounds = Bounds.Create( value, value );
            var range = new[] { bounds, bounds, bounds };

            var result = factory( range );

            using ( new AssertionScope() )
            {
                result.Should().BeSequentiallyEqualTo( bounds );
                result[0].Should().Be( bounds );
            }
        }

        private void Factory_WithRange_ShouldReturnCorrectResult_WhenAllValuesAreUnique(
            Func<IEnumerable<Bounds<T>>, BoundsRange<T>> factory)
        {
            var (min1, max1, min2, max2, min3, max3) = Fixture.CreateDistinctSortedCollection<T>( 6 );
            var bounds1 = Bounds.Create( min1, max1 );
            var bounds2 = Bounds.Create( min2, max2 );
            var bounds3 = Bounds.Create( min3, max3 );
            var range = new[] { bounds1, bounds2, bounds3 };

            var result = factory( range );

            using ( new AssertionScope() )
            {
                result.Should().BeSequentiallyEqualTo( bounds1, bounds2, bounds3 );
                result[0].Should().Be( bounds1 );
                result[1].Should().Be( bounds2 );
                result[2].Should().Be( bounds3 );
            }
        }

        private void Factory_WithRange_ShouldReturnCorrectResult_WhenSomeValuesAreRedundant(
            Func<IEnumerable<Bounds<T>>, BoundsRange<T>> factory)
        {
            var (a, b, c, d) = Fixture.CreateDistinctSortedCollection<T>( 4 );
            var bounds1 = Bounds.Create( a, b );
            var bounds2 = Bounds.Create( b, c );
            var bounds3 = Bounds.Create( d, d );
            var range = new[] { bounds1, bounds2, bounds3 };

            var result = factory( range );

            using ( new AssertionScope() )
            {
                result.Should().BeSequentiallyEqualTo( Bounds.Create( a, c ), bounds3 );
                result[0].Should().Be( Bounds.Create( a, c ) );
                result[1].Should().Be( bounds3 );
            }
        }

        private void Factory_WithRange_ShouldThrowArgumentException_WhenBoundsAreNotOrdered(
            Func<IEnumerable<Bounds<T>>, BoundsRange<T>> factory)
        {
            var (min2, max2, min1, max1) = Fixture.CreateDistinctSortedCollection<T>( 4 );
            var bounds1 = Bounds.Create( min1, max1 );
            var bounds2 = Bounds.Create( min2, max2 );
            var range = new[] { bounds1, bounds2 };

            var action = Lambda.Of( () => factory( range ) );

            action.Should().ThrowExactly<ArgumentException>();
        }
    }
}
