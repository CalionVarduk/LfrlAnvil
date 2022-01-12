using System;
using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.TimeZoneInfo
{
    public class TransitionTimeExtensionsTestsData
    {
        public static TheoryData<System.DateTime, int, System.DateTime> GetToDateTimeWithFixedTimeData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, int, System.DateTime>
            {
                { new System.DateTime( 1, 3, 26, 12, 30, 40, 500 ), 2021, new System.DateTime( 2021, 3, 26, 12, 30, 40, 500 ) },
                { new System.DateTime( 1, 2, 28, 12, 30, 40, 500 ), 2021, new System.DateTime( 2021, 2, 28, 12, 30, 40, 500 ) },
                { new System.DateTime( 2020, 2, 29, 12, 30, 40, 500 ), 2020, new System.DateTime( 2020, 2, 29, 12, 30, 40, 500 ) },
                { new System.DateTime( 2020, 2, 29, 12, 30, 40, 500 ), 2021, new System.DateTime( 2021, 2, 28, 12, 30, 40, 500 ) }
            };
        }

        public static TheoryData<TimeSpan, int, int, System.DayOfWeek, int, System.DateTime> GetToDateTimeWithFloatingTimeData(
            IFixture fixture)
        {
            var timeOfDay = new TimeSpan( 0, 12, 30, 40, 500 );
            var year = 2021;
            var month = 4;
            Func<int, System.DateTime> expectedFactory = day => new System.DateTime( year, month, day ) + timeOfDay;

            return new TheoryData<TimeSpan, int, int, System.DayOfWeek, int, System.DateTime>
            {
                { timeOfDay, month, 1, System.DayOfWeek.Monday, year, expectedFactory( 5 ) },
                { timeOfDay, month, 1, System.DayOfWeek.Tuesday, year, expectedFactory( 6 ) },
                { timeOfDay, month, 1, System.DayOfWeek.Wednesday, year, expectedFactory( 7 ) },
                { timeOfDay, month, 1, System.DayOfWeek.Thursday, year, expectedFactory( 1 ) },
                { timeOfDay, month, 1, System.DayOfWeek.Friday, year, expectedFactory( 2 ) },
                { timeOfDay, month, 1, System.DayOfWeek.Saturday, year, expectedFactory( 3 ) },
                { timeOfDay, month, 1, System.DayOfWeek.Sunday, year, expectedFactory( 4 ) },
                { timeOfDay, month, 2, System.DayOfWeek.Monday, year, expectedFactory( 12 ) },
                { timeOfDay, month, 2, System.DayOfWeek.Tuesday, year, expectedFactory( 13 ) },
                { timeOfDay, month, 2, System.DayOfWeek.Wednesday, year, expectedFactory( 14 ) },
                { timeOfDay, month, 2, System.DayOfWeek.Thursday, year, expectedFactory( 8 ) },
                { timeOfDay, month, 2, System.DayOfWeek.Friday, year, expectedFactory( 9 ) },
                { timeOfDay, month, 2, System.DayOfWeek.Saturday, year, expectedFactory( 10 ) },
                { timeOfDay, month, 2, System.DayOfWeek.Sunday, year, expectedFactory( 11 ) },
                { timeOfDay, month, 3, System.DayOfWeek.Monday, year, expectedFactory( 19 ) },
                { timeOfDay, month, 3, System.DayOfWeek.Tuesday, year, expectedFactory( 20 ) },
                { timeOfDay, month, 3, System.DayOfWeek.Wednesday, year, expectedFactory( 21 ) },
                { timeOfDay, month, 3, System.DayOfWeek.Thursday, year, expectedFactory( 15 ) },
                { timeOfDay, month, 3, System.DayOfWeek.Friday, year, expectedFactory( 16 ) },
                { timeOfDay, month, 3, System.DayOfWeek.Saturday, year, expectedFactory( 17 ) },
                { timeOfDay, month, 3, System.DayOfWeek.Sunday, year, expectedFactory( 18 ) },
                { timeOfDay, month, 4, System.DayOfWeek.Monday, year, expectedFactory( 26 ) },
                { timeOfDay, month, 4, System.DayOfWeek.Tuesday, year, expectedFactory( 27 ) },
                { timeOfDay, month, 4, System.DayOfWeek.Wednesday, year, expectedFactory( 28 ) },
                { timeOfDay, month, 4, System.DayOfWeek.Thursday, year, expectedFactory( 22 ) },
                { timeOfDay, month, 4, System.DayOfWeek.Friday, year, expectedFactory( 23 ) },
                { timeOfDay, month, 4, System.DayOfWeek.Saturday, year, expectedFactory( 24 ) },
                { timeOfDay, month, 4, System.DayOfWeek.Sunday, year, expectedFactory( 25 ) },
                { timeOfDay, month, 5, System.DayOfWeek.Monday, year, expectedFactory( 26 ) },
                { timeOfDay, month, 5, System.DayOfWeek.Tuesday, year, expectedFactory( 27 ) },
                { timeOfDay, month, 5, System.DayOfWeek.Wednesday, year, expectedFactory( 28 ) },
                { timeOfDay, month, 5, System.DayOfWeek.Thursday, year, expectedFactory( 29 ) },
                { timeOfDay, month, 5, System.DayOfWeek.Friday, year, expectedFactory( 30 ) },
                { timeOfDay, month, 5, System.DayOfWeek.Saturday, year, expectedFactory( 24 ) },
                { timeOfDay, month, 5, System.DayOfWeek.Sunday, year, expectedFactory( 25 ) }
            };
        }
    }
}
