using System;
using FluentAssertions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.DayOfWeekTests;

public class DayOfWeekExtensionsTests : TestsBase
{
    [Theory]
    [InlineData( DayOfWeek.Monday, IsoDayOfWeek.Monday )]
    [InlineData( DayOfWeek.Tuesday, IsoDayOfWeek.Tuesday )]
    [InlineData( DayOfWeek.Wednesday, IsoDayOfWeek.Wednesday )]
    [InlineData( DayOfWeek.Thursday, IsoDayOfWeek.Thursday )]
    [InlineData( DayOfWeek.Friday, IsoDayOfWeek.Friday )]
    [InlineData( DayOfWeek.Saturday, IsoDayOfWeek.Saturday )]
    [InlineData( DayOfWeek.Sunday, IsoDayOfWeek.Sunday )]
    public void ToIso_ShouldReturnCorrectResult(DayOfWeek sut, IsoDayOfWeek expected)
    {
        var result = sut.ToIso();
        result.Should().Be( expected );
    }
}