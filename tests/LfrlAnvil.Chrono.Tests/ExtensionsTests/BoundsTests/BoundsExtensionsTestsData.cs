using System;
using AutoFixture;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.BoundsTests
{
    public class BoundsExtensionsTestsData
    {
        public static TheoryData<DateTime, DateTime, TimeSpan> GetGetTimeSpanData(IFixture fixture)
        {
            var dt1 = new DateTime( 2021, 8, 26 );
            var dt2 = new DateTime( 2021, 8, 27, 12, 34, 56, 789 ).AddTicks( 9876 );
            var dt3 = new DateTime( 2021, 8, 27, 13, 34, 56, 789 ).AddTicks( 9875 );

            return new TheoryData<DateTime, DateTime, TimeSpan>
            {
                { dt1, dt1, TimeSpan.FromTicks( 1 ) },
                { dt1, dt2, new TimeSpan( 1, 12, 34, 56, 789 ) + TimeSpan.FromTicks( 9877 ) },
                { dt2, dt3, TimeSpan.FromHours( 1 ) }
            };
        }

        public static TheoryData<DateTime, DateTime, PeriodUnits, Period> GetGetPeriodData(IFixture fixture)
        {
            var dt1 = new DateTime( 2021, 8, 26 );
            var dt2 = new DateTime( 2021, 10, 17, 12, 34, 56, 789 ).AddTicks( 9876 );
            var dt3 = new DateTime( 2023, 9, 17, 13, 34, 56, 789 ).AddTicks( 9875 );

            return new TheoryData<DateTime, DateTime, PeriodUnits, Period>
            {
                { dt1, dt1, PeriodUnits.All, Period.FromTicks( 1 ) },
                { dt1, dt2, PeriodUnits.All, new Period( 0, 1, 3, 1, 12, 34, 56, 789, 9877 ) },
                { dt1, dt2, PeriodUnits.Date, new Period( 0, 1, 3, 1 ) },
                { dt1, dt2, PeriodUnits.Time, new Period( 1260, 34, 56, 789, 9877 ) },
                { dt2, dt3, PeriodUnits.All, new Period( 1, 11, 0, 0, 1, 0, 0, 0, 0 ) },
                { dt2, dt3, PeriodUnits.Date, new Period( 1, 11, 0, 0 ) },
                { dt2, dt3, PeriodUnits.Time, Period.FromHours( 16801 ) }
            };
        }

        public static TheoryData<DateTime, DateTime, PeriodUnits, Period> GetGetGreedyPeriodData(IFixture fixture)
        {
            var dt1 = new DateTime( 2021, 8, 26 );
            var dt2 = new DateTime( 2021, 10, 17, 12, 34, 56, 789 ).AddTicks( 9876 );
            var dt3 = new DateTime( 2023, 9, 17, 13, 34, 56, 789 ).AddTicks( 9875 );

            return new TheoryData<DateTime, DateTime, PeriodUnits, Period>
            {
                { dt1, dt1, PeriodUnits.All, Period.FromTicks( 1 ) },
                { dt1, dt2, PeriodUnits.All, new Period( 0, 2, -1, -2, 12, 34, 56, 789, 9877 ) },
                { dt1, dt2, PeriodUnits.Date, new Period( 0, 2, -1, -2 ) },
                { dt1, dt2, PeriodUnits.Time, new Period( 1260, 34, 56, 789, 9877 ) },
                { dt2, dt3, PeriodUnits.All, new Period( 2, -1, 0, 0, 1, 0, 0, 0, 0 ) },
                { dt2, dt3, PeriodUnits.Date, new Period( 2, -1, 0, 0 ) },
                { dt2, dt3, PeriodUnits.Time, Period.FromHours( 16801 ) }
            };
        }

        public static TheoryData<DateTime, DateTime, Duration> GetGetDurationData(IFixture fixture)
        {
            var dt1 = new DateTime( 2021, 8, 26 );
            var dt2 = new DateTime( 2021, 8, 27, 12, 34, 56, 789 ).AddTicks( 9876 );
            var dt3 = new DateTime( 2021, 8, 27, 13, 34, 56, 789 ).AddTicks( 9875 );

            return new TheoryData<DateTime, DateTime, Duration>
            {
                { dt1, dt1, Duration.FromTicks( 1 ) },
                { dt1, dt2, new Duration( 36, 34, 56, 789, 9877 ) },
                { dt2, dt3, Duration.FromHours( 1 ) }
            };
        }

        public static TheoryData<DateTime, DateTime, PeriodUnits, Period> GetGetPeriodWithZonedDayData(IFixture fixture)
        {
            var dt1 = new DateTime( 2021, 8, 26 );
            var dt2 = new DateTime( 2021, 10, 17 );
            var dt3 = new DateTime( 2023, 9, 17 );

            return new TheoryData<DateTime, DateTime, PeriodUnits, Period>
            {
                { dt1, dt1, PeriodUnits.All, Period.FromDays( 1 ) },
                { dt1, dt2, PeriodUnits.All, new Period( 0, 1, 3, 2 ) },
                { dt1, dt2, PeriodUnits.Date, new Period( 0, 1, 3, 2 ) },
                { dt1, dt2, PeriodUnits.Time, Period.FromHours( 1272 ) },
                { dt2, dt3, PeriodUnits.All, new Period( 1, 11, 0, 1 ) },
                { dt2, dt3, PeriodUnits.Date, new Period( 1, 11, 0, 1 ) },
                { dt2, dt3, PeriodUnits.Time, Period.FromHours( 16824 ) }
            };
        }

        public static TheoryData<DateTime, DateTime, PeriodUnits, Period> GetGetGreedyPeriodWithZonedDayData(IFixture fixture)
        {
            var dt1 = new DateTime( 2021, 8, 26 );
            var dt2 = new DateTime( 2021, 10, 17 );
            var dt3 = new DateTime( 2023, 9, 17 );

            return new TheoryData<DateTime, DateTime, PeriodUnits, Period>
            {
                { dt1, dt1, PeriodUnits.All, Period.FromDays( 1 ) },
                { dt1, dt2, PeriodUnits.All, new Period( 0, 2, -1, -1 ) },
                { dt1, dt2, PeriodUnits.Date, new Period( 0, 2, -1, -1 ) },
                { dt1, dt2, PeriodUnits.Time, Period.FromHours( 1272 ) },
                { dt2, dt3, PeriodUnits.All, new Period( 2, -1, 0, 1 ) },
                { dt2, dt3, PeriodUnits.Date, new Period( 2, -1, 0, 1 ) },
                { dt2, dt3, PeriodUnits.Time, Period.FromHours( 16824 ) }
            };
        }
    }
}
