using System;
using FluentAssertions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.TimestampProvider
{
    public class TimestampProviderTests : TestsBase
    {
        [Fact]
        public void GetNow_ShouldReturnCorrectResult()
        {
            var sut = new Core.Chrono.TimestampProvider();

            var expectedMin = DateTime.UtcNow;
            var result = sut.GetNow();
            var expectedMax = DateTime.UtcNow;

            result.UtcValue.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
        }
    }
}
