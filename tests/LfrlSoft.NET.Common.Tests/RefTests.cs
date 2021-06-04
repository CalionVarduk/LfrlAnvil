using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class RefTests
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
    }
}
