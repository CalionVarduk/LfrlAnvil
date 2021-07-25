using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Collections;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Collections.One
{
    public class Static
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Core.Collections.One.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Core.Collections.One.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( One<int> ), typeof( int ) )]
        [InlineData( typeof( One<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( One<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Core.Collections.One.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( One<> ).GetGenericArguments()[0];

            var result = Core.Collections.One.GetUnderlyingType( typeof( One<> ) );

            result.Should().Be( expected );
        }
    }
}
