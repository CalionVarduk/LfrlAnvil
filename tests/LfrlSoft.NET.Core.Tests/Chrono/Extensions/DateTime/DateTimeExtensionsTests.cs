using LfrlSoft.NET.TestExtensions;
using Xunit;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.TestExtensions.Attributes;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.DateTime
{
    [TestClass( typeof( DateTimeExtensionsTestsData ) )]
    public class DateTimeExtensionsTests : TestsBase
    {
        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetGetMonthOfYearData ) )]
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

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetGetStartOfDayData ) )]
        public void GetStartOfDay_ShouldReturnCorrectResult(System.DateTime value, System.DateTime expected)
        {
            var result = value.GetStartOfDay();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetGetEndOfDayData ) )]
        public void GetEndOfDay_ShouldReturnCorrectResult(System.DateTime value, System.DateTime expected)
        {
            var result = value.GetEndOfDay();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetGetStartOfMonthData ) )]
        public void GetStartOfMonth_ShouldReturnCorrectResult(System.DateTime value, System.DateTime expected)
        {
            var result = value.GetStartOfMonth();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetGetEndOfMonthData ) )]
        public void GetEndOfMonth_ShouldReturnCorrectResult(System.DateTime value, System.DateTime expected)
        {
            var result = value.GetEndOfMonth();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetGetStartOfYearData ) )]
        public void GetStartOfYear_ShouldReturnCorrectResult(System.DateTime value, System.DateTime expected)
        {
            var result = value.GetStartOfYear();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetGetEndOfYearData ) )]
        public void GetEndOfYear_ShouldReturnCorrectResult(System.DateTime value, System.DateTime expected)
        {
            var result = value.GetEndOfYear();
            result.Should().Be( expected );
        }
    }
}
