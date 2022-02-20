using System;
using FluentAssertions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.BoundsRangeTests
{
    public class BoundsRangeStaticTests : TestsBase
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = BoundsRange.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = BoundsRange.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( BoundsRange<int> ), typeof( int ) )]
        [InlineData( typeof( BoundsRange<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( BoundsRange<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = BoundsRange.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( BoundsRange<> ).GetGenericArguments()[0];

            var result = BoundsRange.GetUnderlyingType( typeof( BoundsRange<> ) );

            result.Should().Be( expected );
        }
    }
}
