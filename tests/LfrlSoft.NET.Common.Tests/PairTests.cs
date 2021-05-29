using System;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public class PairTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void CtorWithValue_ShouldCreateWithCorrectValues()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();

            var sut = new Pair<int, string>( first, second );

            sut.First.Should().Be( first );
            sut.Second.Should().Be( second );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var first = 987654321;
            var second = 1234567890;
            var sut = new Pair<int, int>( first, second );

            var result = sut.GetHashCode();

            result.Should().Be( -553869366 );
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenAllPropertiesAreEqual()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();

            var sut = new Pair<int, string>( first, second );

            var result = sut.Equals( new Pair<int, string>( first, second ) );

            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenAnyPropertiesAreDifferent()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();

            var sut = new Pair<int, string>( first, second );

            var result = sut.Equals( new Pair<int, string>( first + 1, second ) );

            result.Should().BeFalse();
        }

        [Fact]
        public void SetFirst_ShouldReturnCorrectResult()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();
            var other = _fixture.Create<decimal>();

            var sut = new Pair<int, string>( first, second );

            var result = sut.SetFirst( other );

            result.First.Should().Be( other );
            result.Second.Should().Be( second );
        }

        [Fact]
        public void SetSecond_ShouldReturnCorrectResult()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();
            var other = _fixture.Create<decimal>();

            var sut = new Pair<int, string>( first, second );

            var result = sut.SetSecond( other );

            result.First.Should().Be( first );
            result.Second.Should().Be( other );
        }

        [Fact]
        public void EqualityOperator_ShouldReturnTrue_WhenAllPropertiesAreEqual()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();

            var sut = new Pair<int, string>( first, second );

            var result = sut == new Pair<int, string>( first, second );

            result.Should().BeTrue();
        }

        [Fact]
        public void EqualityOperator_ShouldReturnFalse_WhenAnyPropertiesAreDifferent()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();

            var sut = new Pair<int, string>( first, second );

            var result = sut == new Pair<int, string>( first + 1, second );

            result.Should().BeFalse();
        }

        [Fact]
        public void InequalityOperator_ShouldReturnTrue_WhenAnyPropertiesAreDifferent()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();

            var sut = new Pair<int, string>( first, second );

            var result = sut != new Pair<int, string>( first + 1, second );

            result.Should().BeTrue();
        }

        [Fact]
        public void InequalityOperator_ShouldReturnFalse_WhenAnyPropertiesAreEqual()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();

            var sut = new Pair<int, string>( first, second );

            var result = sut != new Pair<int, string>( first, second );

            result.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldCreateWithCorrectValues()
        {
            var first = _fixture.Create<int>();
            var second = _fixture.Create<string>();

            var sut = Pair.Create( first, second );

            sut.First.Should().Be( first );
            sut.Second.Should().Be( second );
        }

        [Fact]
        public void GetUnderlyingFirstType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Pair.GetUnderlyingFirstType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingFirstType_ShouldReturnNull_WhenTypeIsNotPair(Type type)
        {
            var result = Pair.GetUnderlyingFirstType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Pair<int, string> ), typeof( int ) )]
        [InlineData( typeof( Pair<decimal, bool> ), typeof( decimal ) )]
        [InlineData( typeof( Pair<double, byte> ), typeof( double ) )]
        public void GetUnderlyingFirstType_ShouldReturnCorrectType_WhenTypeIsPair(Type type, Type expected)
        {
            var result = Pair.GetUnderlyingFirstType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingFirstType_ShouldReturnCorrectType_WhenTypeIsOpenPair()
        {
            var expected = typeof( Pair<,> ).GetGenericArguments()[0];

            var result = Pair.GetUnderlyingFirstType( typeof( Pair<,> ) );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingSecondType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Pair.GetUnderlyingSecondType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingSecondType_ShouldReturnNull_WhenTypeIsNotPair(Type type)
        {
            var result = Pair.GetUnderlyingSecondType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Pair<int, string> ), typeof( string ) )]
        [InlineData( typeof( Pair<decimal, bool> ), typeof( bool ) )]
        [InlineData( typeof( Pair<double, byte> ), typeof( byte ) )]
        public void GetUnderlyingSecondType_ShouldReturnCorrectType_WhenTypeIsPair(Type type, Type expected)
        {
            var result = Pair.GetUnderlyingSecondType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingSecondType_ShouldReturnCorrectType_WhenTypeIsOpenPair()
        {
            var expected = typeof( Pair<,> ).GetGenericArguments()[1];

            var result = Pair.GetUnderlyingSecondType( typeof( Pair<,> ) );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Pair.GetUnderlyingTypes( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNotPair(Type type)
        {
            var result = Pair.GetUnderlyingTypes( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Pair<int, string> ), typeof( int ), typeof( string ) )]
        [InlineData( typeof( Pair<decimal, bool> ), typeof( decimal ), typeof( bool ) )]
        [InlineData( typeof( Pair<double, byte> ), typeof( double ), typeof( byte ) )]
        public void GetUnderlyingTypes_ShouldReturnCorrectType_WhenTypeIsPair(Type type, Type expectedFirst, Type expectedSecond)
        {
            var result = Pair.GetUnderlyingTypes( type );

            result.Should().BeEquivalentTo( new Pair<Type, Type>( expectedFirst, expectedSecond ) );
        }

        [Fact]
        public void GetUnderlyingTypes_ShouldReturnCorrectType_WhenTypeIsOpenPair()
        {
            var expected = typeof( Pair<,> ).GetGenericArguments();

            var result = Pair.GetUnderlyingTypes( typeof( Pair<,> ) );

            result.Should().BeEquivalentTo( new Pair<Type, Type>( expected[0], expected[1] ) );
        }
    }
}
