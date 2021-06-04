using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class PairTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void CtorWithValue_ShouldCreateWithCorrectValues()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();

            var sut = new Pair<int, string>( first, second );

            sut.First.Should().Be( first );
            sut.Second.Should().Be( second );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var first = 987654321;
            var second = 1234567890;
            var sut = new Pair<int, int>( first, second );

            var result = sut.GetHashCode();

            result.Should().Be( -553869366 );
        }

        [Theory]
        [InlineData( 1, "a", 1, "a", true )]
        [InlineData( 1, "a", 1, "b", false )]
        [InlineData( 1, "a", 2, "a", false )]
        [InlineData( 1, "a", 2, "b", false )]
        public void Equals_ShouldReturnCorrectResult(int first1, string second1, int first2, string second2, bool expected)
        {
            var a = new Pair<int, string>( first1, second1 );
            var b = new Pair<int, string>( first2, second2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void SetFirst_ShouldReturnCorrectResult()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();
            var other = _fixture.Create<decimal>();

            var sut = new Pair<int, string>( first, second );

            var result = sut.SetFirst( other );

            result.First.Should().Be( other );
            result.Second.Should().Be( second );
        }

        [Fact]
        public void SetSecond_ShouldReturnCorrectResult()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();
            var other = _fixture.Create<decimal>();

            var sut = new Pair<int, string>( first, second );

            var result = sut.SetSecond( other );

            result.First.Should().Be( first );
            result.Second.Should().Be( other );
        }
    }
}
