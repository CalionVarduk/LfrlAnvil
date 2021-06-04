using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class HashTests
    {
        [Fact]
        public void IntConversionOperator_ShouldReturnUnderlyingValue()
        {
            var value = _fixture.Create<int>();
            var sut = new Hash( value );

            var result = (int) sut;

            result.Should().Be( value );
        }

        [Theory]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, false )]
        public void EqualityOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, false )]
        [InlineData( 1, 2, true )]
        public void InequalityOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a != b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, false )]
        [InlineData( 1, 2, false )]
        [InlineData( 2, 1, true )]
        public void GreaterThanOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a > b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, true )]
        [InlineData( 2, 1, false )]
        public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a <= b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, false )]
        [InlineData( 1, 2, true )]
        [InlineData( 2, 1, false )]
        public void LessThanOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a < b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, false )]
        [InlineData( 2, 1, true )]
        public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a >= b;

            result.Should().Be( expected );
        }
    }
}
