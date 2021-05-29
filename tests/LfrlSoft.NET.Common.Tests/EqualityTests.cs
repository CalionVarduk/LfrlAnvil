using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using System;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public class EqualityTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void Ctor_ShouldCreateWithResultEqualToTrue_WhenBothValuesAreEqual()
        {
            var value = _fixture.Create<int>();

            var sut = new Equality<int>( value, value );

            sut.Should().Match<Equality<int>>( e => e.Result && e.First == value && e.Second == value );
        }

        [Fact]
        public void Ctor_ShouldCreateWithResultEqualToFalse_WhenBothValuesAreDifferent()
        {
            var (value1, value2) = _fixture.CreateDistinctPair<int>();

            var sut = new Equality<int>( value1, value2 );

            sut.Should().Match<Equality<int>>( e => ! e.Result && e.First == value1 && e.Second == value2 );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var value1 = 1234567890;
            var value2 = 987654321;
            var sut = new Equality<int>( value1, value2 );

            var result = sut.GetHashCode();

            result.Should().Be( 2089901860 );
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenAllPropertiesAreEqual()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();
            var sut = new Equality<int>( value1, value2 );

            var result = sut.Equals( new Equality<int>( value1, value2 ) );

            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenAnyPropertiesAreDifferent()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();
            var sut = new Equality<int>( value1, value2 );

            var result = sut.Equals( new Equality<int>( value1, value2 + 1 ) );

            result.Should().BeFalse();
        }

        [Fact]
        public void ImplicitOperator_ShouldReturnUnderlyingResult()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();
            var sut = new Equality<int>( value1, value2 );

            var result = (bool) sut;

            result.Should().Be( sut.Result );
        }

        [Fact]
        public void NegateOperator_ShouldReturnNegatedUnderlyingResult()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();
            var sut = new Equality<int>( value1, value2 );

            var result = ! sut;

            result.Should().Be( ! sut.Result );
        }

        [Fact]
        public void EqualityOperator_ShouldReturnTrue_WhenAllPropertiesAreEqual()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();
            var sut = new Equality<int>( value1, value2 );

            var result = sut == new Equality<int>( value1, value2 );

            result.Should().BeTrue();
        }

        [Fact]
        public void EqualityOperator_ShouldReturnFalse_WhenAnyPropertiesAreDifferent()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();
            var sut = new Equality<int>( value1, value2 );

            var result = sut == new Equality<int>( value1, value2 + 1 );

            result.Should().BeFalse();
        }

        [Fact]
        public void InequalityOperator_ShouldReturnTrue_WhenAnyPropertiesAreDifferent()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();
            var sut = new Equality<int>( value1, value2 );

            var result = sut != new Equality<int>( value1, value2 + 1 );

            result.Should().BeTrue();
        }

        [Fact]
        public void InequalityOperator_ShouldReturnFalse_WhenAllPropertiesAreEqual()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();
            var sut = new Equality<int>( value1, value2 );

            var result = sut != new Equality<int>( value1, value2 );

            result.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldCreateWithCorrectProperties()
        {
            var value1 = _fixture.Create<int>();
            var value2 = _fixture.Create<int>();

            var sut = Equality.Create( value1, value2 );

            sut.Should().Match<Equality<int>>( e => e.First == value1 && e.Second == value2 );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Equality.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNotEquality(Type type)
        {
            var result = Equality.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Equality<int> ), typeof( int ) )]
        [InlineData( typeof( Equality<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Equality<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsEquality(Type type, Type expected)
        {
            var result = Equality.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsOpenEquality()
        {
            var expected = typeof( Equality<> ).GetGenericArguments()[0];

            var result = Equality.GetUnderlyingType( typeof( Equality<> ) );

            result.Should().Be( expected );
        }
    }
}
