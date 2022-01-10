using FluentAssertions;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.IsoDayOfWeek
{
    public class IsoDayOfWeekExtensionsTests : TestsBase
    {
        [Theory]
        [InlineData( Core.Chrono.IsoDayOfWeek.Monday, System.DayOfWeek.Monday )]
        [InlineData( Core.Chrono.IsoDayOfWeek.Tuesday, System.DayOfWeek.Tuesday )]
        [InlineData( Core.Chrono.IsoDayOfWeek.Wednesday, System.DayOfWeek.Wednesday )]
        [InlineData( Core.Chrono.IsoDayOfWeek.Thursday, System.DayOfWeek.Thursday )]
        [InlineData( Core.Chrono.IsoDayOfWeek.Friday, System.DayOfWeek.Friday )]
        [InlineData( Core.Chrono.IsoDayOfWeek.Saturday, System.DayOfWeek.Saturday )]
        [InlineData( Core.Chrono.IsoDayOfWeek.Sunday, System.DayOfWeek.Sunday )]
        public void ToBcl_ShouldReturnCorrectResult(Core.Chrono.IsoDayOfWeek sut, System.DayOfWeek expected)
        {
            var result = sut.ToBcl();
            result.Should().Be( expected );
        }
    }
}
