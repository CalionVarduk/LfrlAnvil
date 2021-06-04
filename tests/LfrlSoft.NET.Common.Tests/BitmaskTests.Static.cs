using System;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class BitmaskTests
    {
        [Fact]
        public void Create_ShouldCreateCorrectBitmask()
        {
            var value = _fixture.Create<int>();

            var sut = Bitmask.Create( value );

            sut.Value.Should().Be( value );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Bitmask.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
        {
            var result = Bitmask.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Bitmask<int> ), typeof( int ) )]
        [InlineData( typeof( Bitmask<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Bitmask<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
        {
            var result = Bitmask.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
        {
            var expected = typeof( Bitmask<> ).GetGenericArguments()[0];

            var result = Bitmask.GetUnderlyingType( typeof( Bitmask<> ) );

            result.Should().Be( expected );
        }
    }
}
