using LfrlSoft.NET.TestExtensions;
using Xunit;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Chrono.Extensions;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.DateTime
{
    public class DateTimeExtensionsTests : TestsBase
    {
        [Theory]
        [InlineData( 1, IsoMonthOfYear.January )]
        [InlineData( 2, IsoMonthOfYear.February )]
        [InlineData( 3, IsoMonthOfYear.March )]
        [InlineData( 4, IsoMonthOfYear.April )]
        [InlineData( 5, IsoMonthOfYear.May )]
        [InlineData( 6, IsoMonthOfYear.June )]
        [InlineData( 7, IsoMonthOfYear.July )]
        [InlineData( 8, IsoMonthOfYear.August )]
        [InlineData( 9, IsoMonthOfYear.September )]
        [InlineData( 10, IsoMonthOfYear.October )]
        [InlineData( 11, IsoMonthOfYear.November )]
        [InlineData( 12, IsoMonthOfYear.December )]
        public void GetMonthOfYear_ShouldReturnCorrectResult(int month, IsoMonthOfYear expected)
        {
            var value = new System.DateTime( 2021, month, 1 );
            var result = value.GetMonthOfYear();
            result.Should().Be( expected );
        }

        [Fact]
        public void GetDayOfWeek_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<System.DateTime>();
            var result = value.GetDayOfWeek();
            result.Should().Be( value.DayOfWeek.ToIso() );
        }
    }
}
