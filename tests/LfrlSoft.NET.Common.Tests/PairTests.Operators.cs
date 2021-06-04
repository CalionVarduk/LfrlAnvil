using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class PairTests
    {
        [Theory]
        [InlineData( 1, "a", 1, "a", true )]
        [InlineData( 1, "a", 1, "b", false )]
        [InlineData( 1, "a", 2, "a", false )]
        [InlineData( 1, "a", 2, "b", false )]
        public void EqualityOperator_ShouldReturnCorrectResult(int first1, string second1, int first2, string second2, bool expected)
        {
            var a = new Pair<int, string>( first1, second1 );
            var b = new Pair<int, string>( first2, second2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, "a", 1, "a", false )]
        [InlineData( 1, "a", 1, "b", true )]
        [InlineData( 1, "a", 2, "a", true )]
        [InlineData( 1, "a", 2, "b", true )]
        public void InequalityOperator_ShouldReturnCorrectResult(int first1, string second1, int first2, string second2, bool expected)
        {
            var a = new Pair<int, string>( first1, second1 );
            var b = new Pair<int, string>( first2, second2 );

            var result = a != b;

            result.Should().Be( expected );
        }
    }
}
