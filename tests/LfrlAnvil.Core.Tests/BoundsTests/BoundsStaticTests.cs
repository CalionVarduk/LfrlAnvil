using System;
using FluentAssertions;
using Xunit;

namespace LfrlAnvil.Tests.BoundsTests
{
    public class BoundsStaticTests
    {
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
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Bounds.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Bounds<int> ), typeof( int ) )]
        [InlineData( typeof( Bounds<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Bounds<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Bounds.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Bounds<> ).GetGenericArguments()[0];

            var result = Bounds.GetUnderlyingType( typeof( Bounds<> ) );

            result.Should().Be( expected );
        }
    }
}
