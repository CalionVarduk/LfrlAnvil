using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Extensions.Nullable
{
    public abstract class NullableExtensionsTests<T> : TestsBase
        where T : struct
    {
        [Fact]
        public void ToNullable_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<T>();

            var result = value.ToNullable();

            result.Should().BeEquivalentTo( value );
        }
    }
}
