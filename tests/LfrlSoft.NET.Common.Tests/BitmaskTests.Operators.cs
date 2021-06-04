using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class BitmaskTests
    {
        [Fact]
        public void TConversionOperator_ShouldReturnUnderlyingValue()
        {
            var value = _fixture.Create<int>();
            var sut = new Bitmask<int>( value );

            var result = (int) sut;

            result.Should().Be( value );
        }

        [Fact]
        public void BitmaskConversionOperator_ShouldCreateProperBitmask()
        {
            var value = _fixture.Create<int>();

            var result = (Bitmask<int>) value;

            result.Value.Should().Be( value );
        }

        [Theory]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, false )]
        public void EqualityOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Bitmask<int>( value1 );
            var b = new Bitmask<int>( value2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, false )]
        [InlineData( 1, 2, true )]
        public void InequalityOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Bitmask<int>( value1 );
            var b = new Bitmask<int>( value2 );

            var result = a != b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, false )]
        [InlineData( 1, 2, false )]
        [InlineData( 2, 1, true )]
        public void GreaterThanOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Bitmask<int>( value1 );
            var b = new Bitmask<int>( value2 );

            var result = a > b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, true )]
        [InlineData( 2, 1, false )]
        public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Bitmask<int>( value1 );
            var b = new Bitmask<int>( value2 );

            var result = a <= b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, false )]
        [InlineData( 1, 2, true )]
        [InlineData( 2, 1, false )]
        public void LessThanOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Bitmask<int>( value1 );
            var b = new Bitmask<int>( value2 );

            var result = a < b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, false )]
        [InlineData( 2, 1, true )]
        public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Bitmask<int>( value1 );
            var b = new Bitmask<int>( value2 );

            var result = a >= b;

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
        public void BitwiseOrOperator_ShouldReturnCorrectResult(uint value1, uint value2, uint expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a | b;

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
        public void BitwiseAndOperator_ShouldReturnCorrectResult(uint value1, uint value2, uint expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a & b;

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
        public void BitwiseXorOperator_ShouldReturnCorrectResult(uint value1, uint value2, uint expected)
        {
            var a = new Bitmask<uint>( value1 );
            var b = new Bitmask<uint>( value2 );

            var result = a ^ b;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0U, 4294967295U )]
        [InlineData( 1U, 4294967294U )]
        [InlineData( 2U, 4294967293U )]
        [InlineData( 3U, 4294967292U )]
        public void BitwiseNegateOperator_ShouldReturnCorrectResult(uint value, uint expected)
        {
            var sut = new Bitmask<uint>( value );

            var result = ~sut;

            result.Value.Should().Be( expected );
        }
    }
}
