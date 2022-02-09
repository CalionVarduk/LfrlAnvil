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

        public static TheoryData<System.DateTime, System.DayOfWeek, System.DateTime> GetGetStartOfWeekData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, System.DayOfWeek, System.DateTime>
            {
                { new System.DateTime( 2021, 3, 26 ), System.DayOfWeek.Monday, new System.DateTime( 2021, 3, 22 ) },
                { new System.DateTime( 2021, 3, 26 ), System.DayOfWeek.Tuesday, new System.DateTime( 2021, 3, 23 ) },
                { new System.DateTime( 2021, 3, 26 ), System.DayOfWeek.Wednesday, new System.DateTime( 2021, 3, 24 ) },
                { new System.DateTime( 2021, 3, 26 ), System.DayOfWeek.Thursday, new System.DateTime( 2021, 3, 25 ) },
                { new System.DateTime( 2021, 3, 26 ), System.DayOfWeek.Friday, new System.DateTime( 2021, 3, 26 ) },
                { new System.DateTime( 2021, 3, 26 ), System.DayOfWeek.Saturday, new System.DateTime( 2021, 3, 20 ) },
                { new System.DateTime( 2021, 3, 26 ), System.DayOfWeek.Sunday, new System.DateTime( 2021, 3, 21 ) },
                { new System.DateTime( 2021, 3, 26, 12, 0, 0 ), System.DayOfWeek.Monday, new System.DateTime( 2021, 3, 22 ) },
                { new System.DateTime( 2021, 3, 26, 12, 0, 0 ), System.DayOfWeek.Tuesday, new System.DateTime( 2021, 3, 23 ) },
                { new System.DateTime( 2021, 3, 26, 12, 0, 0 ), System.DayOfWeek.Wednesday, new System.DateTime( 2021, 3, 24 ) },
                { new System.DateTime( 2021, 3, 26, 12, 0, 0 ), System.DayOfWeek.Thursday, new System.DateTime( 2021, 3, 25 ) },
                { new System.DateTime( 2021, 3, 26, 12, 0, 0 ), System.DayOfWeek.Friday, new System.DateTime( 2021, 3, 26 ) },
                { new System.DateTime( 2021, 3, 26, 12, 0, 0 ), System.DayOfWeek.Saturday, new System.DateTime( 2021, 3, 20 ) },
                { new System.DateTime( 2021, 3, 26, 12, 0, 0 ), System.DayOfWeek.Sunday, new System.DateTime( 2021, 3, 21 ) },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Monday,
                    new System.DateTime( 2021, 3, 22 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Tuesday,
                    new System.DateTime( 2021, 3, 23 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Wednesday,
                    new System.DateTime( 2021, 3, 24 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Thursday,
                    new System.DateTime( 2021, 3, 25 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Friday,
                    new System.DateTime( 2021, 3, 26 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Saturday,
                    new System.DateTime( 2021, 3, 20 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Sunday,
                    new System.DateTime( 2021, 3, 21 )
                }
            };
        }

        public static TheoryData<System.DateTime, System.DayOfWeek, System.DateTime> GetGetEndOfWeekData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, System.DayOfWeek, System.DateTime>
            {
                {
                    new System.DateTime( 2021, 3, 26 ),
                    System.DayOfWeek.Monday,
                    new System.DateTime( 2021, 3, 28, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26 ),
                    System.DayOfWeek.Tuesday,
                    new System.DateTime( 2021, 3, 29, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26 ),
                    System.DayOfWeek.Wednesday,
                    new System.DateTime( 2021, 3, 30, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26 ),
                    System.DayOfWeek.Thursday,
                    new System.DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26 ),
                    System.DayOfWeek.Friday,
                    new System.DateTime( 2021, 4, 1, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26 ),
                    System.DayOfWeek.Saturday,
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26 ),
                    System.DayOfWeek.Sunday,
                    new System.DateTime( 2021, 3, 27, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 12, 0, 0 ),
                    System.DayOfWeek.Monday,
                    new System.DateTime( 2021, 3, 28, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 12, 0, 0 ),
                    System.DayOfWeek.Tuesday,
                    new System.DateTime( 2021, 3, 29, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 12, 0, 0 ),
                    System.DayOfWeek.Wednesday,
                    new System.DateTime( 2021, 3, 30, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 12, 0, 0 ),
                    System.DayOfWeek.Thursday,
                    new System.DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 12, 0, 0 ),
                    System.DayOfWeek.Friday,
                    new System.DateTime( 2021, 4, 1, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 12, 0, 0 ),
                    System.DayOfWeek.Saturday,
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 12, 0, 0 ),
                    System.DayOfWeek.Sunday,
                    new System.DateTime( 2021, 3, 27, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Monday,
                    new System.DateTime( 2021, 3, 28, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Tuesday,
                    new System.DateTime( 2021, 3, 29, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Wednesday,
                    new System.DateTime( 2021, 3, 30, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Thursday,
                    new System.DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Friday,
                    new System.DateTime( 2021, 4, 1, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Saturday,
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    System.DayOfWeek.Sunday,
                    new System.DateTime( 2021, 3, 27, 23, 59, 59, 999 ).AddTicks( 9999 )
                }
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

        public static TheoryData<System.DateTime, Core.Chrono.Period, System.DateTime> GetAddData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, Core.Chrono.Period, System.DateTime>
            {
                {
                    new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    Core.Chrono.Period.Empty,
                    new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    new Core.Chrono.Period( 1, 2, 3, 4, 5 ),
                    new System.DateTime( 2021, 8, 26, 13, 32, 43, 504 ).AddTicks( 6006 )
                },
                {
                    new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    new Core.Chrono.Period( -1, -2, -3, -4, -5 ),
                    new System.DateTime( 2021, 8, 26, 11, 28, 37, 496 ).AddTicks( 5996 )
                },
                {
                    new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    new Core.Chrono.Period( 1, 2, 3, 4 ),
                    new System.DateTime( 2022, 11, 20, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    new Core.Chrono.Period( -1, -2, -3, -4 ),
                    new System.DateTime( 2020, 6, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new System.DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    Core.Chrono.Period.FromMonths( 1 ),
                    new System.DateTime( 2021, 4, 30, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new System.DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    Core.Chrono.Period.FromMonths( -1 ),
                    new System.DateTime( 2021, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new System.DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    Core.Chrono.Period.FromYears( -1 ).SubtractMonths( 1 ),
                    new System.DateTime( 2020, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    new Core.Chrono.Period( 1, 2, 3, 4, 5, 6, 7, 8, 9 ),
                    new System.DateTime( 2022, 11, 20, 17, 36, 47, 508 ).AddTicks( 6010 )
                },
                {
                    new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                    new Core.Chrono.Period( 1, -2, 3, -4, 5, -6, 7, -8, 9 ),
                    new System.DateTime( 2022, 7, 13, 17, 24, 47, 492 ).AddTicks( 6010 )
                },
                {
                    new System.DateTime( 2021, 7, 25, 11, 59, 59, 999 ).AddTicks( 9998 ),
                    Core.Chrono.Period.FromMonths( 1 ).AddDays( 1 ).AddTicks( 1 ),
                    new System.DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    new System.DateTime( 2021, 9, 27, 13, 0, 0 ).AddTicks( 1 ),
                    Core.Chrono.Period.FromMonths( -1 ).SubtractDays( 1 ).SubtractTicks( 1 ),
                    new System.DateTime( 2021, 8, 26, 13, 0, 0 )
                }
            };
        }

        public static TheoryData<System.DateTime, int, System.DateTime> GetSetYearData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, int, System.DateTime>
            {
                { new System.DateTime( 2021, 8, 26 ), 2021, new System.DateTime( 2021, 8, 26 ) },
                { new System.DateTime( 2021, 8, 26 ), 2022, new System.DateTime( 2022, 8, 26 ) },
                { new System.DateTime( 2021, 8, 26 ), 2020, new System.DateTime( 2020, 8, 26 ) },
                { new System.DateTime( 2020, 2, 29 ), 2021, new System.DateTime( 2021, 2, 28 ) },
                { new System.DateTime( 2021, 8, 26 ), 2019, new System.DateTime( 2019, 8, 26 ) },
                { new System.DateTime( 2021, 8, 25 ), 2019, new System.DateTime( 2019, 8, 25 ) }
            };
        }

        public static TheoryData<System.DateTime, int> GetSetYearThrowData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, int>
            {
                { fixture.Create<System.DateTime>(), System.DateTime.MinValue.Year - 1 },
                { fixture.Create<System.DateTime>(), System.DateTime.MaxValue.Year + 1 }
            };
        }

        public static TheoryData<System.DateTime, IsoMonthOfYear, System.DateTime> GetSetMonthData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, IsoMonthOfYear, System.DateTime>
            {
                { new System.DateTime( 2021, 8, 26 ), IsoMonthOfYear.August, new System.DateTime( 2021, 8, 26 ) },
                { new System.DateTime( 2021, 8, 26 ), IsoMonthOfYear.September, new System.DateTime( 2021, 9, 26 ) },
                { new System.DateTime( 2021, 8, 26 ), IsoMonthOfYear.July, new System.DateTime( 2021, 7, 26 ) },
                { new System.DateTime( 2021, 8, 31 ), IsoMonthOfYear.April, new System.DateTime( 2021, 4, 30 ) },
                { new System.DateTime( 2021, 8, 31 ), IsoMonthOfYear.February, new System.DateTime( 2021, 2, 28 ) },
                { new System.DateTime( 2020, 8, 31 ), IsoMonthOfYear.February, new System.DateTime( 2020, 2, 29 ) },
                { new System.DateTime( 2021, 6, 26 ), IsoMonthOfYear.August, new System.DateTime( 2021, 8, 26 ) },
                { new System.DateTime( 2021, 6, 25 ), IsoMonthOfYear.August, new System.DateTime( 2021, 8, 25 ) }
            };
        }

        public static TheoryData<System.DateTime, int, System.DateTime> GetSetDayOfMonthData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, int, System.DateTime>
            {
                { new System.DateTime( 2021, 8, 26 ), 26, new System.DateTime( 2021, 8, 26 ) },
                { new System.DateTime( 2021, 8, 26 ), 1, new System.DateTime( 2021, 8, 1 ) },
                { new System.DateTime( 2021, 8, 26 ), 31, new System.DateTime( 2021, 8, 31 ) },
                { new System.DateTime( 2021, 8, 16 ), 26, new System.DateTime( 2021, 8, 26 ) },
                { new System.DateTime( 2021, 8, 16 ), 25, new System.DateTime( 2021, 8, 25 ) }
            };
        }

        public static TheoryData<System.DateTime, int> GetSetDayOfMonthThrowData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, int>
            {
                { new System.DateTime( 2021, 1, 1 ), 0 },
                { new System.DateTime( 2021, 2, 1 ), 0 },
                { new System.DateTime( 2021, 3, 1 ), 0 },
                { new System.DateTime( 2021, 4, 1 ), 0 },
                { new System.DateTime( 2021, 5, 1 ), 0 },
                { new System.DateTime( 2021, 6, 1 ), 0 },
                { new System.DateTime( 2021, 7, 1 ), 0 },
                { new System.DateTime( 2021, 8, 1 ), 0 },
                { new System.DateTime( 2021, 9, 1 ), 0 },
                { new System.DateTime( 2021, 10, 1 ), 0 },
                { new System.DateTime( 2021, 11, 1 ), 0 },
                { new System.DateTime( 2021, 12, 1 ), 0 },
                { new System.DateTime( 2021, 1, 1 ), Constants.DaysInJanuary + 1 },
                { new System.DateTime( 2021, 2, 1 ), Constants.DaysInFebruary + 1 },
                { new System.DateTime( 2020, 2, 1 ), Constants.DaysInLeapFebruary + 1 },
                { new System.DateTime( 2021, 3, 1 ), Constants.DaysInMarch + 1 },
                { new System.DateTime( 2021, 4, 1 ), Constants.DaysInApril + 1 },
                { new System.DateTime( 2021, 5, 1 ), Constants.DaysInMay + 1 },
                { new System.DateTime( 2021, 6, 1 ), Constants.DaysInJune + 1 },
                { new System.DateTime( 2021, 7, 1 ), Constants.DaysInJuly + 1 },
                { new System.DateTime( 2021, 8, 1 ), Constants.DaysInAugust + 1 },
                { new System.DateTime( 2021, 9, 1 ), Constants.DaysInSeptember + 1 },
                { new System.DateTime( 2021, 10, 1 ), Constants.DaysInOctober + 1 },
                { new System.DateTime( 2021, 11, 1 ), Constants.DaysInNovember + 1 },
                { new System.DateTime( 2021, 12, 1 ), Constants.DaysInDecember + 1 },
            };
        }

        public static TheoryData<System.DateTime, int, System.DateTime> GetSetDayOfYearData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, int, System.DateTime>
            {
                { new System.DateTime( 2021, 8, 26 ), 1, new System.DateTime( 2021, 1, 1 ) },
                { new System.DateTime( 2021, 8, 26 ), 365, new System.DateTime( 2021, 12, 31 ) },
                { new System.DateTime( 2020, 8, 26 ), 365, new System.DateTime( 2020, 12, 30 ) },
                { new System.DateTime( 2020, 8, 26 ), 366, new System.DateTime( 2020, 12, 31 ) },
                { new System.DateTime( 2021, 8, 26 ), 238, new System.DateTime( 2021, 8, 26 ) },
                { new System.DateTime( 2020, 8, 26 ), 239, new System.DateTime( 2020, 8, 26 ) },
                { new System.DateTime( 2021, 6, 16 ), 238, new System.DateTime( 2021, 8, 26 ) },
                { new System.DateTime( 2021, 6, 16 ), 237, new System.DateTime( 2021, 8, 25 ) }
            };
        }

        public static TheoryData<System.DateTime, int> GetSetDayOfYearThrowData(IFixture fixture)
        {
            return new TheoryData<System.DateTime, int>
            {
                { new System.DateTime( 2021, 1, 1 ), 0 },
                { new System.DateTime( 2021, 1, 1 ), Constants.DaysInYear + 1 },
                { new System.DateTime( 2020, 1, 1 ), Constants.DaysInLeapYear + 1 }
            };
        }

        public static TheoryData<System.DateTime, Core.Chrono.TimeOfDay, System.DateTime> GetSetTimeOfDayData(IFixture fixture)
        {
            var dt = new System.DateTime( 2021, 8, 26, 17, 18, 19, 234 ).AddTicks( 5678 );

            return new TheoryData<System.DateTime, Core.Chrono.TimeOfDay, System.DateTime>
            {
                { dt, Core.Chrono.TimeOfDay.Start, new System.DateTime( 2021, 8, 26 ) },
                { dt, Core.Chrono.TimeOfDay.Mid, new System.DateTime( 2021, 8, 26, 12, 0, 0 ) },
                { dt, Core.Chrono.TimeOfDay.End, new System.DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ) },
                {
                    dt,
                    new Core.Chrono.TimeOfDay( 12, 30, 40, 500, 6001 ),
                    new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
                },
                {
                    dt,
                    new Core.Chrono.TimeOfDay( 11, 59, 59, 999, 9999 ),
                    new System.DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
                },
                {
                    dt,
                    new Core.Chrono.TimeOfDay( 13 ),
                    new System.DateTime( 2021, 8, 26, 13, 0, 0 )
                }
            };
        }
    }
}
