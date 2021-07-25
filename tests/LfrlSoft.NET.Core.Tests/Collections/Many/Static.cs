using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Collections;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Collections.Many
{
    public class Static
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Core.Collections.Many.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Core.Collections.Many.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Many<int> ), typeof( int ) )]
        [InlineData( typeof( Many<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Many<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Core.Collections.Many.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Many<> ).GetGenericArguments()[0];

            var result = Core.Collections.Many.GetUnderlyingType( typeof( Many<> ) );

            result.Should().Be( expected );
        }
    }
}
