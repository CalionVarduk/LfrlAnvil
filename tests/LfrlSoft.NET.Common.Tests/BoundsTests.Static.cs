using System;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class BoundsTests
    {
        [Fact]
        public void Create_ShouldCreateCorrectBounds()
        {
            var (min, max) = _fixture.CreateDistinctPair<int>();

            var sut = Bounds.Create( min, max );

            sut.Min.Should().Be( min );
            sut.Max.Should().Be( max );
        }

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
