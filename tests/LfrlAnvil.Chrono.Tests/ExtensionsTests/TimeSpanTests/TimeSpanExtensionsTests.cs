using System;
using FluentAssertions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.TimeSpanTests;

public class TimeSpanExtensionsTests : TestsBase
{
    [Theory]
    [InlineData( 10, 10 )]
    [InlineData( 1, 1 )]
    [InlineData( 0, 0 )]
    [InlineData( -1, 1 )]
    [InlineData( -10, 10 )]
    public void Abs_ShouldReturnCorrectResult(int ticks, int expectedTicks)
    {
        var sut = TimeSpan.FromTicks( ticks );
        var result = sut.Abs();
        result.Ticks.Should().Be( expectedTicks );
    }
}