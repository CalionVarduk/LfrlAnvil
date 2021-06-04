using AutoFixture;
using FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public partial class RefTests
    {
        [Fact]
        public void TConversionOperator_ShouldReturnUnderlyingValue()
        {
            var value = _fixture.Create<int>();
            var sut = new Ref<int>( value );

            var result = (int) sut;

            result.Should().Be( value );
        }

        [Fact]
        public void RefConversionOperator_ShouldCreateProperRef()
        {
            var value = _fixture.Create<int>();

            var result = (Ref<int>) value;

            result.Value.Should().Be( value );
        }
    }
}
