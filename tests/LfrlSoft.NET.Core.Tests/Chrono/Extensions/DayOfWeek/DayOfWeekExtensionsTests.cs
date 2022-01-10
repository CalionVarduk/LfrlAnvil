using FluentAssertions;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.DayOfWeek
{
    public class DayOfWeekExtensionsTests : TestsBase
    {
        [Theory]
        [InlineData( System.DayOfWeek.Monday, Core.Chrono.IsoDayOfWeek.Monday )]
        [InlineData( System.DayOfWeek.Tuesday, Core.Chrono.IsoDayOfWeek.Tuesday )]
        [InlineData( System.DayOfWeek.Wednesday, Core.Chrono.IsoDayOfWeek.Wednesday )]
        [InlineData( System.DayOfWeek.Thursday, Core.Chrono.IsoDayOfWeek.Thursday )]
        [InlineData( System.DayOfWeek.Friday, Core.Chrono.IsoDayOfWeek.Friday )]
        [InlineData( System.DayOfWeek.Saturday, Core.Chrono.IsoDayOfWeek.Saturday )]
        [InlineData( System.DayOfWeek.Sunday, Core.Chrono.IsoDayOfWeek.Sunday )]
        public void ToIso_ShouldReturnCorrectResult(System.DayOfWeek sut, Core.Chrono.IsoDayOfWeek expected)
        {
            var result = sut.ToIso();
            result.Should().Be( expected );
        }
    }
}
