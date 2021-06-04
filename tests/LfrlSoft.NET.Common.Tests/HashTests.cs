using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class HashTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void Ctor_ShouldCreateWithCorrectValue()
        {
            var value = _fixture.Create<int>();

            var sut = new Hash( value );

            sut.Value.Should().Be( value );
        }

        [Fact]
        public void Add_ShouldCreateCorrectHashInstance_WhenParameterIsNull()
        {
            var value = _fixture.CreateDefault<string>();
            var sut = Hash.Default;

            var result = sut.Add( value );

            result.Value.Should().Be( 84696351 );
        }

        [Fact]
        public void Add_ShouldCreateCorrectHashInstance_WhenParameterIsNotNull()
        {
            var value = 1234567890;
            var sut = Hash.Default;

            var result = sut.Add( value );

            result.Value.Should().Be( -919047883 );
        }

        [Fact]
        public void AddRange_ShouldCreateCorrectHashInstance()
        {
            var range = new[] { 1234567890, 987654321, 1010101010 };
            var sut = Hash.Default;

            var result = sut.AddRange( range );

            result.Value.Should().Be( 104542330 );
        }

        [Fact]
        public void GetHashCode_ShouldReturnValue()
        {
            var value = _fixture.Create<int>();
            var sut = new Hash( value );

            var result = sut.GetHashCode();

            result.Should().Be( value );
        }

        [Theory]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, false )]
        public void Equals_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 1, 1, 0 )]
        [InlineData( 1, 2, -1 )]
        [InlineData( 2, 1, 1 )]
        public void CompareTo_ShouldReturnCorrectResult(int value1, int value2, int expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a.CompareTo( b );

            result.Should().Be( expected );
        }
    }
}
