using AutoFixture;
using LfrlSoft.NET.Core.Chrono;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.DateTime
{
    public class DateTimeExtensionsTestsData
    {
        public static TheoryData<int, IsoMonthOfYear> GetGetMonthOfYearData(IFixture fixture)
        {
            return new TheoryData<int, IsoMonthOfYear>
            {
                { 1, IsoMonthOfYear.January },
                { 2, IsoMonthOfYear.February },
                { 3, IsoMonthOfYear.March },
                { 4, IsoMonthOfYear.April },
                { 5, IsoMonthOfYear.May },
                { 6, IsoMonthOfYear.June },
                { 7, IsoMonthOfYear.July },
                { 8, IsoMonthOfYear.August },
                { 9, IsoMonthOfYear.September },
                { 10, IsoMonthOfYear.October },
                { 11, IsoMonthOfYear.November },
                { 12, IsoMonthOfYear.December }
            };
        }

        public static TheoryData<System.DateTime, System.DateTime> GetGetStartOfDayData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, System.DateTime>
            {
                { new System.DateTime( 2021, 3, 26 ), new System.DateTime( 2021, 3, 26 ) },
                { new System.DateTime( 2021, 3, 26, 12, 0, 0 ), new System.DateTime( 2021, 3, 26 ) },
                { new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), new System.DateTime( 2021, 3, 26 ) }
            };
        }

        public static TheoryData<System.DateTime, System.DateTime> GetGetEndOfDayData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, System.DateTime>
            {
                { new System.DateTime( 2021, 3, 26 ), new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                { new System.DateTime( 2021, 3, 26, 12, 0, 0 ), new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
            };
        }

        public static TheoryData<System.DateTime, System.DateTime> GetGetStartOfMonthData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, System.DateTime>
            {
                { new System.DateTime( 2021, 3, 1 ), new System.DateTime( 2021, 3, 1 ) },
                { new System.DateTime( 2021, 3, 15, 12, 0, 0 ), new System.DateTime( 2021, 3, 1 ) },
                { new System.DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), new System.DateTime( 2021, 3, 1 ) },
                { new System.DateTime( 2021, 2, 28, 23, 59, 59, 999 ).AddTicks( 9999 ), new System.DateTime( 2021, 2, 1 ) },
                { new System.DateTime( 2020, 2, 29, 23, 59, 59, 999 ).AddTicks( 9999 ), new System.DateTime( 2020, 2, 1 ) },
                { new System.DateTime( 2021, 4, 30, 23, 59, 59, 999 ).AddTicks( 9999 ), new System.DateTime( 2021, 4, 1 ) }
            };
        }

        public static TheoryData<System.DateTime, System.DateTime> GetGetEndOfMonthData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, System.DateTime>
            {
                { new System.DateTime( 2021, 3, 1 ), new System.DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                { new System.DateTime( 2021, 3, 15, 12, 0, 0 ), new System.DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                {
                    new System.DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    new System.DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                { new System.DateTime( 2021, 2, 1 ), new System.DateTime( 2021, 2, 28, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                { new System.DateTime( 2020, 2, 1 ), new System.DateTime( 2020, 2, 29, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                { new System.DateTime( 2021, 4, 1 ), new System.DateTime( 2021, 4, 30, 23, 59, 59, 999 ).AddTicks( 9999 ) }
            };
        }

        public static TheoryData<System.DateTime, System.DateTime> GetGetStartOfYearData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, System.DateTime>
            {
                { new System.DateTime( 2021, 1, 1 ), new System.DateTime( 2021, 1, 1 ) },
                { new System.DateTime( 2021, 6, 26, 12, 0, 0 ), new System.DateTime( 2021, 1, 1 ) },
                { new System.DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), new System.DateTime( 2021, 1, 1 ) }
            };
        }

        public static TheoryData<System.DateTime, System.DateTime> GetGetEndOfYearData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, System.DateTime>
            {
                { new System.DateTime( 2021, 1, 1 ), new System.DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                { new System.DateTime( 2021, 6, 26, 12, 0, 0 ), new System.DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                {
                    new System.DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    new System.DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
            };
        }
    }
}
