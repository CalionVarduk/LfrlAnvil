using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.ExtensionsTests.TimeSpanTests
{
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
}
