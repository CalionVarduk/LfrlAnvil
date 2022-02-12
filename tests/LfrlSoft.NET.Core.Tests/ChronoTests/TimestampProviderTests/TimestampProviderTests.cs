using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.TimestampProviderTests
{
    public class TimestampProviderTests : TestsBase
    {
        [Fact]
        public void GetNow_ShouldReturnCorrectResult()
        {
            var sut = new TimestampProvider();

            var expectedMin = DateTime.UtcNow;
            var result = sut.GetNow();
            var expectedMax = DateTime.UtcNow;

            result.UtcValue.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
        }
    }
}
