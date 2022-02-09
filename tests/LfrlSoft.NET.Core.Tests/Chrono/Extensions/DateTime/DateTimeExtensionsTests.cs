using System;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Chrono.Extensions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

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
        [MethodData( nameof( DateTimeExtensionsTestsData.GetGetStartOfWeekData ) )]
        public void GetStartOfWeek_ShouldReturnCorrectResult(System.DateTime value, System.DayOfWeek weekStart, System.DateTime expected)
        {
            var result = value.GetStartOfWeek( weekStart );
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetGetEndOfWeekData ) )]
        public void GetEndOfWeek_ShouldReturnCorrectResult(System.DateTime value, System.DayOfWeek weekStart, System.DateTime expected)
        {
            var result = value.GetEndOfWeek( weekStart );
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

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetAddData ) )]
        public void Add_ShouldReturnCorrectResult(System.DateTime value, Core.Chrono.Period period, System.DateTime expected)
        {
            var result = value.Add( period );
            result.Should().Be( expected );
        }

        [Fact]
        public void Subtract_ShouldReturnCorrectResult()
        {
            var sut = Fixture.Create<System.DateTime>();
            var periodToSubtract = new Core.Chrono.Period(
                years: Fixture.Create<sbyte>(),
                months: Fixture.Create<sbyte>(),
                weeks: Fixture.Create<short>(),
                days: Fixture.Create<short>(),
                hours: Fixture.Create<short>(),
                minutes: Fixture.Create<short>(),
                seconds: Fixture.Create<short>(),
                milliseconds: Fixture.Create<short>(),
                ticks: Fixture.Create<short>() );

            var expected = sut.Add( -periodToSubtract );

            var result = sut.Subtract( periodToSubtract );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetSetYearData ) )]
        public void SetYear_ShouldReturnCorrectResult(System.DateTime value, int year, System.DateTime expected)
        {
            var result = value.SetYear( year );
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetSetYearThrowData ) )]
        public void SetYear_ShouldThrowArgumentOutOfRangeException_WhenYearIsInvalid(System.DateTime value, int year)
        {
            var action = Lambda.Of( () => value.SetYear( year ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetSetMonthData ) )]
        public void SetMonth_ShouldReturnCorrectResult(System.DateTime value, IsoMonthOfYear month, System.DateTime expected)
        {
            var result = value.SetMonth( month );
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetSetDayOfMonthData ) )]
        public void SetDayOfMonth_ShouldReturnCorrectResult(System.DateTime value, int day, System.DateTime expected)
        {
            var result = value.SetDayOfMonth( day );
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetSetDayOfMonthThrowData ) )]
        public void SetDayOfMonth_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(System.DateTime value, int day)
        {
            var action = Lambda.Of( () => value.SetDayOfMonth( day ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetSetDayOfYearData ) )]
        public void SetDayOfYear_ShouldReturnCorrectResult(System.DateTime value, int day, System.DateTime expected)
        {
            var result = value.SetDayOfYear( day );
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetSetDayOfYearThrowData ) )]
        public void SetDayOfYear_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(System.DateTime value, int day)
        {
            var action = Lambda.Of( () => value.SetDayOfYear( day ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( DateTimeExtensionsTestsData.GetSetTimeOfDayData ) )]
        public void SetTimeOfDay_ShouldReturnCorrectResult(System.DateTime value, Core.Chrono.TimeOfDay timeOfDay, System.DateTime expected)
        {
            var result = value.SetTimeOfDay( timeOfDay );
            result.Should().Be( expected );
        }
    }
}
