using System;
using FluentAssertions;
using Xunit;

namespace LfrlAnvil.Tests.OneTests
{
    public class OneStaticTests
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = One.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = One.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( One<int> ), typeof( int ) )]
        [InlineData( typeof( One<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( One<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = One.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( One<> ).GetGenericArguments()[0];

            var result = One.GetUnderlyingType( typeof( One<> ) );

            result.Should().Be( expected );
        }
    }
}
