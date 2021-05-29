using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public class BoundsTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void Ctor_ShouldCreateWithDistinctMinAndMax()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            sut.Min.Should().Be( min );
            sut.Max.Should().Be( max );
        }

        [Fact]
        public void Ctor_ShouldCreateWithTheSameMinAndMax()
        {
            var value = _fixture.Create<int>();

            var sut = new Bounds<int>( value, value );

            sut.Min.Should().Be( value );
            sut.Max.Should().Be( value );
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenMinIsNull()
        {
            var max = _fixture.Create<string>();

            Action action = () => new Bounds<string>( null!, max );

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenMaxIsNull()
        {
            var min = _fixture.Create<string>();

            Action action = () => new Bounds<string>( min, null! );

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenMinIsGreaterThanMax()
        {
            var (max, min) = _fixture.CreateDistinctPair<int>();

            Action action = () => new Bounds<int>( min, max );

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var min = 987654321;
            var max = 1234567890;
            var sut = new Bounds<int>( min, max );

            var result = sut.GetHashCode();

            result.Should().Be( -553869366 );
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenAllPropertiesAreEqual()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Equals( new Bounds<int>( min, max ) );

            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenAnyPropertiesAreDifferent()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Equals( new Bounds<int>( min, max + 1 ) );

            result.Should().BeFalse();
        }

        [Fact]
        public void SetMin_ShouldReturnCorrectResult()
        {
            var (min, newMin, max) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.SetMin( newMin );

            result.Min.Should().Be( newMin );
            result.Max.Should().Be( max );
        }

        [Fact]
        public void SetMax_ShouldReturnCorrectResult()
        {
            var (min, max, newMax) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.SetMax( newMax );

            result.Min.Should().Be( min );
            result.Max.Should().Be( newMax );
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenValueEqualsMin()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Contains( min );

            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenValueEqualsMax()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Contains( max );

            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenValueIsBetweenMinAndMax()
        {
            var (min, value, max) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Contains( value );

            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnFalse_WhenValueIsLessThanMin()
        {
            var (value, min, max) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Contains( value );

            result.Should().BeFalse();
        }

        [Fact]
        public void Contains_ShouldReturnFalse_WhenValueIsGreaterThanMax()
        {
            var (min, max, value) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Contains( value );

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( 1, 4, 1, 4, true )]
        [InlineData( 1, 4, 1, 3, true )]
        [InlineData( 1, 4, 2, 4, true )]
        [InlineData( 1, 4, 2, 3, true )]
        [InlineData( 1, 4, 0, 4, false )]
        [InlineData( 1, 4, 1, 5, false )]
        [InlineData( 1, 4, 0, 5, false )]
        [InlineData( 1, 4, -1, 0, false )]
        [InlineData( 1, 4, -1, 1, false )]
        [InlineData( 1, 4, 5, 6, false )]
        [InlineData( 1, 4, 4, 6, false )]
        public void Contains_ForBounds_ShouldReturnCorrectResult(int min1, int max1, int min2, int max2, bool expected)
        {
            var sut = new Bounds<int>( min1, max1 );

            var result = sut.Contains( new Bounds<int>( min2, max2 ) );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 4, 1, 4, true )]
        [InlineData( 1, 4, 1, 3, true )]
        [InlineData( 1, 4, 2, 4, true )]
        [InlineData( 1, 4, 2, 3, true )]
        [InlineData( 1, 4, 0, 4, true )]
        [InlineData( 1, 4, 1, 5, true )]
        [InlineData( 1, 4, 0, 5, true )]
        [InlineData( 1, 4, -1, 0, false )]
        [InlineData( 1, 4, -1, 1, true )]
        [InlineData( 1, 4, 5, 6, false )]
        [InlineData( 1, 4, 4, 6, true )]
        public void Intersects_ShouldReturnCorrectResult(int min1, int max1, int min2, int max2, bool expected)
        {
            var sut = new Bounds<int>( min1, max1 );

            var result = sut.Intersects( new Bounds<int>( min2, max2 ) );

            result.Should().Be( expected );
        }

        [Theory]
        [MemberData(nameof(GetIntersectionTestData))]
        public void GetIntersection_ShouldReturnCorrectResult(int min1, int max1, int min2, int max2, Bounds<int>? expected)
        {
            var sut = new Bounds<int>( min1, max1 );

            var result = sut.GetIntersection( new Bounds<int>( min2, max2 ) );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void Clamp_ShouldReturnMin_WhenValueIsLessThanMin()
        {
            var (value, min, max) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Clamp( value );

            result.Should().Be( min );
        }

        [Fact]
        public void Clamp_ShouldReturnMin_WhenValueIsEqualToMin()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Clamp( min );

            result.Should().Be( min );
        }

        [Fact]
        public void Clamp_ShouldReturnMax_WhenValueIsGreaterThanMax()
        {
            var (min, max, value) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Clamp( value );

            result.Should().Be( max );
        }

        [Fact]
        public void Clamp_ShouldReturnMax_WhenValueIsEqualToMax()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Clamp( max );

            result.Should().Be( max );
        }

        [Fact]
        public void Clamp_ShouldReturnValue_WhenValueIsBetweenMinAndMax()
        {
            var (min, value, max) = _fixture.CreateDistinctTriple<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut.Clamp( value );

            result.Should().Be( value );
        }

        [Fact]
        public void EqualityOperator_ShouldReturnTrue_WhenAllPropertiesAreEqual()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut == new Bounds<int>( min, max );

            result.Should().BeTrue();
        }

        [Fact]
        public void EqualityOperator_ShouldReturnFalse_WhenAnyPropertiesAreDifferent()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut == new Bounds<int>( min, max + 1 );

            result.Should().BeFalse();
        }

        [Fact]
        public void InequalityOperator_ShouldReturnTrue_WhenAnyPropertiesAreDifferent()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut != new Bounds<int>( min, max + 1 );

            result.Should().BeTrue();
        }

        [Fact]
        public void InequalityOperator_ShouldReturnFalse_WhenAnyPropertiesAreEqual()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = new Bounds<int>( min, max );

            var result = sut != new Bounds<int>( min, max );

            result.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldCreateWithCorrectProperties()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = Bounds.Create( min, max );

            sut.Min.Should().Be( min );
            sut.Max.Should().Be( max );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Bounds.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNotBounds(Type type)
        {
            var result = Bounds.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Bounds<int> ), typeof( int ) )]
        [InlineData( typeof( Bounds<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Bounds<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsBounds(Type type, Type expected)
        {
            var result = Bounds.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsOpenBounds()
        {
            var expected = typeof( Bounds<> ).GetGenericArguments()[0];

            var result = Bounds.GetUnderlyingType( typeof( Bounds<> ) );

            result.Should().Be( expected );
        }

        public static IEnumerable<object?[]> GetIntersectionTestData()
        {
            return new[]
            {
                new object?[] { 1, 4, 1, 4, new Bounds<int>( 1, 4 ) },
                new object?[] { 1, 4, 1, 3, new Bounds<int>( 1, 3 ) },
                new object?[] { 1, 4, 2, 4, new Bounds<int>( 2, 4 ) },
                new object?[] { 1, 4, 2, 3, new Bounds<int>( 2, 3 ) },
                new object?[] { 1, 4, 0, 4, new Bounds<int>( 1, 4 ) },
                new object?[] { 1, 4, 1, 5, new Bounds<int>( 1, 4 ) },
                new object?[] { 1, 4, 0, 5, new Bounds<int>( 1, 4 ) },
                new object?[] { 1, 4, -1, 0, null },
                new object?[] { 1, 4, -1, 1, new Bounds<int>( 1, 1 ) },
                new object?[] { 1, 4, 5, 6, null },
                new object?[] { 1, 4, 4, 6, new Bounds<int>( 4, 4 ) }
            };
        }
    }
}
