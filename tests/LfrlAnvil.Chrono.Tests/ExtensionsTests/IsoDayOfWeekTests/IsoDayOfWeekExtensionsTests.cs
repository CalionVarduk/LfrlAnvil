using System;
using FluentAssertions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.IsoDayOfWeekTests;

public class IsoDayOfWeekExtensionsTests : TestsBase
{
    [Theory]
    [InlineData( IsoDayOfWeek.Monday, DayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday, DayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday, DayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday, DayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday, DayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday, DayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday, DayOfWeek.Sunday )]
    public void ToBcl_ShouldReturnCorrectResult(IsoDayOfWeek sut, DayOfWeek expected)
    {
        var result = sut.ToBcl();
        result.Should().Be( expected );
    }
}