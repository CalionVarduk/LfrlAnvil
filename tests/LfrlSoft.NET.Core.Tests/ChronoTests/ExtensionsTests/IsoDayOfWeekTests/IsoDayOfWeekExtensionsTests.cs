﻿using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.ExtensionsTests.IsoDayOfWeekTests
{
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
}
