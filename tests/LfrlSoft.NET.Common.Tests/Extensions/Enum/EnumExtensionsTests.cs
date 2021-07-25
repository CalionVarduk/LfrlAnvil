using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Extensions.Enum
{
    public abstract class EnumExtensionsTests<T> : TestsBase
        where T : struct, System.Enum
    {
        [Fact]
        public void ToBitmask_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<T>();

            var result = value.ToBitmask();

            result.Value.Should().Be( value );
        }
    }
}
