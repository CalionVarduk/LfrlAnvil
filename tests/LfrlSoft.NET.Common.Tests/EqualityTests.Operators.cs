using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class EqualityTests
    {
        [Theory]
        [InlineData( 1, 1 )]
        [InlineData( 1, 2 )]
        public void BoolConversionOperator_ShouldReturnUnderlyingResult(int first, int second)
        {
            var sut = new Equality<int>( first, second );

            var result = (bool) sut;

            result.Should().Be( sut.Result );
        }

        [Theory]
        [InlineData( 1, 1 )]
        [InlineData( 1, 2 )]
        public void NegateOperator_ShouldReturnNegatedUnderlyingResult(int first, int second)
        {
            var sut = new Equality<int>( first, second );

            var result = ! sut;

            result.Should().Be( ! sut.Result );
        }

        [Theory]
        [InlineData( 1, 2, 1, 2, true )]
        [InlineData( 1, 2, 1, 3, false )]
        [InlineData( 0, 2, 1, 2, false )]
        [InlineData( 1, 2, 3, 4, false )]
        public void EqualityOperator_ShouldReturnCorrectResult(int first1, int second1, int first2, int second2, bool expected)
        {
            var a = new Equality<int>( first1, second1 );
            var b = new Equality<int>( first2, second2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 2, 1, 2, false )]
        [InlineData( 1, 2, 1, 3, true )]
        [InlineData( 0, 2, 1, 2, true )]
        [InlineData( 1, 2, 3, 4, true )]
        public void InequalityOperator_ShouldReturnCorrectResult(int first1, int second1, int first2, int second2, bool expected)
        {
            var a = new Equality<int>( first1, second1 );
            var b = new Equality<int>( first2, second2 );

            var result = a != b;

            result.Should().Be( expected );
        }
    }
}
