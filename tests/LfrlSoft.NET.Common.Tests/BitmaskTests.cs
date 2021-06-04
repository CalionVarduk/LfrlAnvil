using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class BitmaskTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void Ctor_ShouldCreateWithCorrectValue()
        {
            var value = _fixture.Create<uint>();

            var sut = new Bitmask<uint>( value );

            sut.Value.Should().Be( value );
        }

        [Fact]
        public void GetHashCode_ShouldReturnValue()
        {
            var value = _fixture.Create<uint>();
            var sut = new Bitmask<uint>( value );

            var result = sut.GetHashCode();

            result.Should().Be( value.GetHashCode() );
        }

        [Theory]
        [InlineData( 1U, 1U, true )]
        [InlineData( 1U, 2U, false )]
        public void Equals_ShouldReturnCorrectResult(uint value1, uint value2, bool expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1U, 1U, 0 )]
        [InlineData( 1U, 2U, -1 )]
        [InlineData( 2U, 1U, 1 )]
        public void CompareTo_ShouldReturnCorrectResult(uint value1, uint value2, int expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a.CompareTo( b );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0U, 0U, true )]
        [InlineData( 0U, 1U, false )]
        [InlineData( 1U, 0U, true )]
        [InlineData( 1U, 1U, true )]
        [InlineData( 1U, 2U, false )]
        [InlineData( 1U, 3U, true )]
        [InlineData( 2U, 1U, false )]
        [InlineData( 2U, 2U, true )]
        [InlineData( 2U, 3U, true )]
        [InlineData( 3U, 1U, true )]
        [InlineData( 3U, 2U, true )]
        [InlineData( 3U, 3U, true )]
        public void ContainsAny_ShouldReturnCorrectResult(uint value1, uint value2, bool expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a.ContainsAny( b );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0U, 0U, true )]
        [InlineData( 0U, 1U, false )]
        [InlineData( 1U, 0U, true )]
        [InlineData( 1U, 1U, true )]
        [InlineData( 1U, 2U, false )]
        [InlineData( 1U, 3U, false )]
        [InlineData( 2U, 1U, false )]
        [InlineData( 2U, 2U, true )]
        [InlineData( 2U, 3U, false )]
        [InlineData( 3U, 1U, true )]
        [InlineData( 3U, 2U, true )]
        [InlineData( 3U, 3U, true )]
        public void ContainsAll_ShouldReturnCorrectResult(uint value1, uint value2, bool expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a.ContainsAll( b );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0U, 0U, 0U )]
        [InlineData( 0U, 1U, 1U )]
        [InlineData( 1U, 0U, 1U )]
        [InlineData( 1U, 1U, 1U )]
        [InlineData( 1U, 2U, 3U )]
        [InlineData( 1U, 3U, 3U )]
        [InlineData( 2U, 1U, 3U )]
        [InlineData( 2U, 2U, 2U )]
        [InlineData( 2U, 3U, 3U )]
        [InlineData( 3U, 1U, 3U )]
        [InlineData( 3U, 2U, 3U )]
        [InlineData( 3U, 3U, 3U )]
        public void Set_ShouldReturnCorrectResult(uint value1, uint value2, uint expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a.Set( b );

            result.Value.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0U, 0U, 0U )]
        [InlineData( 0U, 1U, 0U )]
        [InlineData( 1U, 0U, 1U )]
        [InlineData( 1U, 1U, 0U )]
        [InlineData( 1U, 2U, 1U )]
        [InlineData( 1U, 3U, 0U )]
        [InlineData( 2U, 1U, 2U )]
        [InlineData( 2U, 2U, 0U )]
        [InlineData( 2U, 3U, 0U )]
        [InlineData( 3U, 1U, 2U )]
        [InlineData( 3U, 2U, 1U )]
        [InlineData( 3U, 3U, 0U )]
        public void Unset_ShouldReturnCorrectResult(uint value1, uint value2, uint expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a.Unset( b );

            result.Value.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0U, 0U, 0U )]
        [InlineData( 0U, 1U, 0U )]
        [InlineData( 1U, 0U, 0U )]
        [InlineData( 1U, 1U, 1U )]
        [InlineData( 1U, 2U, 0U )]
        [InlineData( 1U, 3U, 1U )]
        [InlineData( 2U, 1U, 0U )]
        [InlineData( 2U, 2U, 2U )]
        [InlineData( 2U, 3U, 2U )]
        [InlineData( 3U, 1U, 1U )]
        [InlineData( 3U, 2U, 2U )]
        [InlineData( 3U, 3U, 3U )]
        public void Intersect_ShouldReturnCorrectResult(uint value1, uint value2, uint expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a.Intersect( b );

            result.Value.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0U, 0U, 0U )]
        [InlineData( 0U, 1U, 1U )]
        [InlineData( 1U, 0U, 1U )]
        [InlineData( 1U, 1U, 0U )]
        [InlineData( 1U, 2U, 3U )]
        [InlineData( 1U, 3U, 2U )]
        [InlineData( 2U, 1U, 3U )]
        [InlineData( 2U, 2U, 0U )]
        [InlineData( 2U, 3U, 1U )]
        [InlineData( 3U, 1U, 2U )]
        [InlineData( 3U, 2U, 1U )]
        [InlineData( 3U, 3U, 0U )]
        public void Alternate_ShouldReturnCorrectResult(uint value1, uint value2, uint expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a.Alternate( b );

            result.Value.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0U, 4294967295U )]
        [InlineData( 1U, 4294967294U )]
        [InlineData( 2U, 4294967293U )]
        [InlineData( 3U, 4294967292U )]
        public void Negate_ShouldReturnCorrectResult(uint value, uint expected)
        {
            var sut = new Bitmask<uint>( value );

            var result = sut.Negate();

            result.Value.Should().Be( expected );
        }

        [Fact]
        public void Clear_ShouldReturnBitmaskWithZeroValue()
        {
            var value = _fixture.Create<uint>();
            var sut = new Bitmask<uint>( value );

            var result = sut.Clear();

            result.Value.Should().Be( 0U );
        }

        [Fact]
        public void Sanitize_ShouldReturnUnderlyingValue()
        {
            var value = _fixture.Create<uint>();
            var sut = new Bitmask<uint>( value );

            var result = sut.Sanitize();

            result.Value.Should().Be( value );
        }

        // TODO: add Signed int tests (negate/sanitize might be enough?)
        // TODO: add BitCount test (via reflection)
        // TODO: add enum tests

        // TODO: in general, refactor unit tests further & create them via abstract generic classes, so that we test for multiple different types
        // TODO: for operator theory tests, create separate X.Data.cs files & have them share their data with their corresponding methods, so that there is no necessary copy-pasting
    }
}
