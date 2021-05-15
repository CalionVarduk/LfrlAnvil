using AutoFixture;
using FluentAssertions;
using System;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public class RefTests
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void ParameterlessCtor_ShouldCreateWithDefaultValue()
        {
            var sut = new Ref<int>();

            sut.Value.Should().Be( default );
        }

        [Fact]
        public void CtorWithValue_ShouldCreateWithCorrectValue()
        {
            var value = _fixture.Create<int>();

            var sut = new Ref<int>( value );

            sut.Value.Should().Be( value );
        }

        [Fact]
        public void ImplicitOperator_ShouldReturnUnderlyingValue()
        {
            var value = _fixture.Create<int>();
            var sut = new Ref<int>( value );

            var result = ( int )sut;

            result.Should().Be( value );
        }

        [Fact]
        public void Create_ShouldCreateWithCorrectValue()
        {
            var value = _fixture.Create<int>();

            var sut = Ref.Create( value );

            sut.Value.Should().Be( value );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
        {
            var result = Ref.GetUnderlyingType( null );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( int ) )]
        [InlineData( typeof( IEquatable<int> ) )]
        [InlineData( typeof( IEquatable<> ) )]
        public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNotRef(Type type)
        {
            var result = Ref.GetUnderlyingType( type );

            result.Should().BeNull();
        }

        [Theory]
        [InlineData( typeof( Ref<int> ), typeof( int ) )]
        [InlineData( typeof( Ref<decimal> ), typeof( decimal ) )]
        [InlineData( typeof( Ref<double> ), typeof( double ) )]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsRef(Type type, Type expected)
        {
            var result = Ref.GetUnderlyingType( type );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsOpenRef()
        {
            var expected = typeof( Ref<> ).GetGenericArguments()[0];

            var result = Ref.GetUnderlyingType( typeof( Ref<> ) );

            result.Should().Be( expected );
        }
    }
}
