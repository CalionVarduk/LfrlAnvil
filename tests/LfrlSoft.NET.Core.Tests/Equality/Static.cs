using System;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Equality
{
    public class Static
    {
        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Core.Equality.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNotCorrect(Type type)
        {
            var result = Core.Equality.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Equality<int> ), typeof( int ) )]
        [InlineData( typeof( Equality<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Equality<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Core.Equality.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Equality<> ).GetGenericArguments()[0];

            var result = Core.Equality.GetUnderlyingType( typeof( Equality<> ) );

            result.Should().Be( expected );
        }
    }
}
