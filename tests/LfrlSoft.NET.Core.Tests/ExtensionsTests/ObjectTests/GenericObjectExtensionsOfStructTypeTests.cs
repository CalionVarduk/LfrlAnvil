using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ExtensionsTests.ObjectTests
{
    public abstract class GenericObjectExtensionsOfStructTypeTests<T> : GenericObjectExtensionsTests<T>
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
