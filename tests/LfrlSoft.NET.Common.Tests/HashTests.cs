using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public class HashTests
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
            var obj = _fixture.CreateDefault<Test>();
            var sut = Hash.Default;

            var result = sut.Add( obj );

            result.Value.Should().Be( 84696351 );
        }

        [Fact]
        public void Add_ShouldCreateCorrectHashInstance_WhenParameterIsNotNull()
        {
            var obj = new Test( 1234567890 );
            var sut = Hash.Default;

            var result = sut.Add( obj );

            result.Value.Should().Be( -919047883 );
        }

        [Fact]
        public void AddRange_ShouldCreateCorrectHashInstance()
        {
            var range = new[] { new Test( 1234567890 ), new Test( 987654321 ), new Test( 1010101010 ) };
            var sut = Hash.Default;

            var result = sut.AddRange( range );

            result.Value.Should().Be( 104542330 );
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenValuesAreEqual()
        {
            var value = _fixture.Create<int>();

            var a = new Hash( value );
            var b = new Hash( value );

            var result = a.Equals( b );

            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenValuesAreDifferent()
        {
            var value = _fixture.Create<int>();

            var a = new Hash( value );
            var b = new Hash( value + 1 );

            var result = a.Equals( b );

            result.Should().BeFalse();
        }

        [Fact]
        public void CompareTo_ShouldReturnZero_WhenValuesAreEqual()
        {
            var value = _fixture.Create<int>();

            var a = new Hash( value );
            var b = new Hash( value );

            var result = a.CompareTo( b );

            result.Should().Be( 0 );
        }

        [Fact]
        public void CompareTo_ShouldReturnOne_WhenOtherValueIsLesser()
        {
            var (value1, value2) = _fixture.CreateDistinctPair<int>();
            var a = new Hash( value2 );
            var b = new Hash( value1 );

            var result = a.CompareTo( b );

            result.Should().Be( 1 );
        }

        [Fact]
        public void CompareTo_ShouldReturnMinusOne_WhenOtherValueIsGreater()
        {
            var (value1, value2) = _fixture.CreateDistinctPair<int>();
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a.CompareTo( b );

            result.Should().Be( -1 );
        }

        [Fact]
        public void ImplicitOperator_ShouldReturnUnderlyingValue()
        {
            var value = _fixture.Create<int>();
            var sut = new Hash( value );

            var result = ( int )sut;

            result.Should().Be( value );
        }

        private class Test
        {
            public Test(int hash)
            {
                Hash = hash;
            }

            public int Hash { get; }

            public override int GetHashCode()
            {
                return Hash;
            }
        }
    }
}
