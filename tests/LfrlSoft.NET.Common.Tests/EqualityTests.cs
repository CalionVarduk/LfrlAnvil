using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class EqualityTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Theory]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, false )]
        public void Ctor_ShouldCreateWithCorrectResult(int first, int second, bool expected)
        {
            var sut = new Equality<int>( first, second );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        First = first,
                        Second = second,
                        Result = expected
                    } );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var value1 = 1234567890;
            var value2 = 987654321;
            var sut = new Equality<int>( value1, value2 );

            var result = sut.GetHashCode();

            result.Should().Be( 2089901860 );
        }

        [Theory]
        [InlineData( 1, 2, 1, 2, true )]
        [InlineData( 1, 2, 1, 3, false )]
        [InlineData( 0, 2, 1, 2, false )]
        [InlineData( 1, 2, 3, 4, false )]
        public void Equals_ShouldReturnCorrectResult(int first1, int second1, int first2, int second2, bool expected)
        {
            var a = new Equality<int>( first1, second1 );
            var b = new Equality<int>( first2, second2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }
    }
}
