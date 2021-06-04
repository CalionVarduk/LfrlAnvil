using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class BoundsTests
    {
        [Theory]
        [InlineData( 1, 2, 1, 2, true )]
        [InlineData( 1, 2, 1, 3, false )]
        [InlineData( 0, 2, 1, 2, false )]
        [InlineData( 1, 2, 3, 4, false )]
        public void EqualityOperator_ShouldReturnCorrectResult(int min1, int max1, int min2, int max2, bool expected)
        {
            var a = new Bounds<int>( min1, max1 );
            var b = new Bounds<int>( min2, max2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 2, 1, 2, false )]
        [InlineData( 1, 2, 1, 3, true )]
        [InlineData( 0, 2, 1, 2, true )]
        [InlineData( 1, 2, 3, 4, true )]
        public void InequalityOperator_ShouldReturnCorrectResult(int min1, int max1, int min2, int max2, bool expected)
        {
            var a = new Bounds<int>( min1, max1 );
            var b = new Bounds<int>( min2, max2 );

            var result = a != b;

            result.Should().Be( expected );
        }
    }
}
