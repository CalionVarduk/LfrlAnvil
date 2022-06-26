using System;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.EnumTests;

public abstract class GenericEnumExtensionsTests<T> : TestsBase
    where T : struct, Enum
{
    [Fact]
    public void ToBitmask_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();
        var result = value.ToBitmask();
        result.Value.Should().Be( value );
    }
}
