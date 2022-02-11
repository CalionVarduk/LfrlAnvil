using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Ref
{
    public abstract class GenericRefTests<T> : TestsBase
        where T : struct
    {
        [Fact]
        public void Create_ShouldCreateCorrectRef()
        {
            var value = Fixture.Create<T>();
            var sut = Core.Ref.Create( value );
            sut.Value.Should().Be( value );
        }

        [Fact]
        public void ParameterlessCtor_ShouldCreateWithDefaultValue()
        {
            var sut = new Ref<T>();
            sut.Value.Should().Be( default( T ) );
        }

        [Fact]
        public void CtorWithValue_ShouldCreateWithCorrectValue()
        {
            var value = Fixture.Create<T>();
            var sut = new Ref<T>( value );
            sut.Value.Should().Be( value );
        }

        [Fact]
        public void TConversionOperator_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<T>();
            var sut = new Ref<T>( value );

            var result = (T)sut;

            result.Should().Be( value );
        }

        [Fact]
        public void RefConversionOperator_ShouldCreateProperRef()
        {
            var value = Fixture.Create<T>();
            var result = (Ref<T>)value;
            result.Value.Should().Be( value );
        }
    }
}
