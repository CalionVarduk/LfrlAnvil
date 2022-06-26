using System;
using AutoFixture;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.DateTimeTests;

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

    public static TheoryData<DateTime, DateTime> GetGetStartOfDayData(IFixture fixture)
    {
        return new TheoryData<DateTime, DateTime>
        {
            { new DateTime( 2021, 3, 26 ), new DateTime( 2021, 3, 26 ) },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), new DateTime( 2021, 3, 26 ) },
            { new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ), new DateTime( 2021, 3, 26 ) }
        };
    }

    public static TheoryData<DateTime, DateTime> GetGetEndOfDayData(IFixture fixture)
    {
        return new TheoryData<DateTime, DateTime>
        {
            { new DateTime( 2021, 3, 26 ), new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
        };
    }

    public static TheoryData<DateTime, DayOfWeek, DateTime> GetGetStartOfWeekData(IFixture fixture)
    {
        return new TheoryData<DateTime, DayOfWeek, DateTime>
        {
            { new DateTime( 2021, 3, 26 ), DayOfWeek.Monday, new DateTime( 2021, 3, 22 ) },
            { new DateTime( 2021, 3, 26 ), DayOfWeek.Tuesday, new DateTime( 2021, 3, 23 ) },
            { new DateTime( 2021, 3, 26 ), DayOfWeek.Wednesday, new DateTime( 2021, 3, 24 ) },
            { new DateTime( 2021, 3, 26 ), DayOfWeek.Thursday, new DateTime( 2021, 3, 25 ) },
            { new DateTime( 2021, 3, 26 ), DayOfWeek.Friday, new DateTime( 2021, 3, 26 ) },
            { new DateTime( 2021, 3, 26 ), DayOfWeek.Saturday, new DateTime( 2021, 3, 20 ) },
            { new DateTime( 2021, 3, 26 ), DayOfWeek.Sunday, new DateTime( 2021, 3, 21 ) },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), DayOfWeek.Monday, new DateTime( 2021, 3, 22 ) },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), DayOfWeek.Tuesday, new DateTime( 2021, 3, 23 ) },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), DayOfWeek.Wednesday, new DateTime( 2021, 3, 24 ) },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), DayOfWeek.Thursday, new DateTime( 2021, 3, 25 ) },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), DayOfWeek.Friday, new DateTime( 2021, 3, 26 ) },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), DayOfWeek.Saturday, new DateTime( 2021, 3, 20 ) },
            { new DateTime( 2021, 3, 26, 12, 0, 0 ), DayOfWeek.Sunday, new DateTime( 2021, 3, 21 ) },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Monday,
                new DateTime( 2021, 3, 22 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Tuesday,
                new DateTime( 2021, 3, 23 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Wednesday,
                new DateTime( 2021, 3, 24 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Thursday,
                new DateTime( 2021, 3, 25 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Friday,
                new DateTime( 2021, 3, 26 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Saturday,
                new DateTime( 2021, 3, 20 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Sunday,
                new DateTime( 2021, 3, 21 )
            }
        };
    }

    public static TheoryData<DateTime, DayOfWeek, DateTime> GetGetEndOfWeekData(IFixture fixture)
    {
        return new TheoryData<DateTime, DayOfWeek, DateTime>
        {
            {
                new DateTime( 2021, 3, 26 ),
                DayOfWeek.Monday,
                new DateTime( 2021, 3, 28, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26 ),
                DayOfWeek.Tuesday,
                new DateTime( 2021, 3, 29, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26 ),
                DayOfWeek.Wednesday,
                new DateTime( 2021, 3, 30, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26 ),
                DayOfWeek.Thursday,
                new DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26 ),
                DayOfWeek.Friday,
                new DateTime( 2021, 4, 1, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26 ),
                DayOfWeek.Saturday,
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26 ),
                DayOfWeek.Sunday,
                new DateTime( 2021, 3, 27, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                DayOfWeek.Monday,
                new DateTime( 2021, 3, 28, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                DayOfWeek.Tuesday,
                new DateTime( 2021, 3, 29, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                DayOfWeek.Wednesday,
                new DateTime( 2021, 3, 30, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                DayOfWeek.Thursday,
                new DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                DayOfWeek.Friday,
                new DateTime( 2021, 4, 1, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                DayOfWeek.Saturday,
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 12, 0, 0 ),
                DayOfWeek.Sunday,
                new DateTime( 2021, 3, 27, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Monday,
                new DateTime( 2021, 3, 28, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Tuesday,
                new DateTime( 2021, 3, 29, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Wednesday,
                new DateTime( 2021, 3, 30, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Thursday,
                new DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Friday,
                new DateTime( 2021, 4, 1, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Saturday,
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 3, 26, 23, 59, 59, 999 ).AddTicks( 9999 ),
                DayOfWeek.Sunday,
                new DateTime( 2021, 3, 27, 23, 59, 59, 999 ).AddTicks( 9999 )
            }
        };
    }

    public static TheoryData<DateTime, DateTime> GetGetStartOfMonthData(IFixture fixture)
    {
        return new TheoryData<DateTime, DateTime>
        {
            { new DateTime( 2021, 3, 1 ), new DateTime( 2021, 3, 1 ) },
            { new DateTime( 2021, 3, 15, 12, 0, 0 ), new DateTime( 2021, 3, 1 ) },
            { new DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), new DateTime( 2021, 3, 1 ) },
            { new DateTime( 2021, 2, 28, 23, 59, 59, 999 ).AddTicks( 9999 ), new DateTime( 2021, 2, 1 ) },
            { new DateTime( 2020, 2, 29, 23, 59, 59, 999 ).AddTicks( 9999 ), new DateTime( 2020, 2, 1 ) },
            { new DateTime( 2021, 4, 30, 23, 59, 59, 999 ).AddTicks( 9999 ), new DateTime( 2021, 4, 1 ) }
        };
    }

    public static TheoryData<DateTime, DateTime> GetGetEndOfMonthData(IFixture fixture)
    {
        return new TheoryData<DateTime, DateTime>
        {
            { new DateTime( 2021, 3, 1 ), new DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            { new DateTime( 2021, 3, 15, 12, 0, 0 ), new DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            {
                new DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                new DateTime( 2021, 3, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
            { new DateTime( 2021, 2, 1 ), new DateTime( 2021, 2, 28, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            { new DateTime( 2020, 2, 1 ), new DateTime( 2020, 2, 29, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            { new DateTime( 2021, 4, 1 ), new DateTime( 2021, 4, 30, 23, 59, 59, 999 ).AddTicks( 9999 ) }
        };
    }

    public static TheoryData<DateTime, DateTime> GetGetStartOfYearData(IFixture fixture)
    {
        return new TheoryData<DateTime, DateTime>
        {
            { new DateTime( 2021, 1, 1 ), new DateTime( 2021, 1, 1 ) },
            { new DateTime( 2021, 6, 26, 12, 0, 0 ), new DateTime( 2021, 1, 1 ) },
            { new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ), new DateTime( 2021, 1, 1 ) }
        };
    }

    public static TheoryData<DateTime, DateTime> GetGetEndOfYearData(IFixture fixture)
    {
        return new TheoryData<DateTime, DateTime>
        {
            { new DateTime( 2021, 1, 1 ), new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            { new DateTime( 2021, 6, 26, 12, 0, 0 ), new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            {
                new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 )
            },
        };
    }

    public static TheoryData<DateTime, Period, DateTime> GetAddData(IFixture fixture)
    {
        return new TheoryData<DateTime, Period, DateTime>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                Period.Empty,
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new Period( 1, 2, 3, 4, 5 ),
                new DateTime( 2021, 8, 26, 13, 32, 43, 504 ).AddTicks( 6006 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new Period( -1, -2, -3, -4, -5 ),
                new DateTime( 2021, 8, 26, 11, 28, 37, 496 ).AddTicks( 5996 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new Period( 1, 2, 3, 4 ),
                new DateTime( 2022, 11, 20, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new Period( -1, -2, -3, -4 ),
                new DateTime( 2020, 6, 1, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                Period.FromMonths( 1 ),
                new DateTime( 2021, 4, 30, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                Period.FromMonths( -1 ),
                new DateTime( 2021, 2, 28, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 3, 31, 12, 30, 40, 500 ).AddTicks( 6001 ),
                Period.FromYears( -1 ).SubtractMonths( 1 ),
                new DateTime( 2020, 2, 29, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new Period( 1, 2, 3, 4, 5, 6, 7, 8, 9 ),
                new DateTime( 2022, 11, 20, 17, 36, 47, 508 ).AddTicks( 6010 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new Period( 1, -2, 3, -4, 5, -6, 7, -8, 9 ),
                new DateTime( 2022, 7, 13, 17, 24, 47, 492 ).AddTicks( 6010 )
            },
            {
                new DateTime( 2021, 7, 25, 11, 59, 59, 999 ).AddTicks( 9998 ),
                Period.FromMonths( 1 ).AddDays( 1 ).AddTicks( 1 ),
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                new DateTime( 2021, 9, 27, 13, 0, 0 ).AddTicks( 1 ),
                Period.FromMonths( -1 ).SubtractDays( 1 ).SubtractTicks( 1 ),
                new DateTime( 2021, 8, 26, 13, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, int, DateTime> GetSetYearData(IFixture fixture)
    {
        return new TheoryData<DateTime, int, DateTime>
        {
            { new DateTime( 2021, 8, 26 ), 2021, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 8, 26 ), 2022, new DateTime( 2022, 8, 26 ) },
            { new DateTime( 2021, 8, 26 ), 2020, new DateTime( 2020, 8, 26 ) },
            { new DateTime( 2020, 2, 29 ), 2021, new DateTime( 2021, 2, 28 ) },
            { new DateTime( 2021, 8, 26 ), 2019, new DateTime( 2019, 8, 26 ) },
            { new DateTime( 2021, 8, 25 ), 2019, new DateTime( 2019, 8, 25 ) }
        };
    }

    public static TheoryData<DateTime, int> GetSetYearThrowData(IFixture fixture)
    {
        return new TheoryData<DateTime, int>
        {
            { fixture.Create<DateTime>(), DateTime.MinValue.Year - 1 },
            { fixture.Create<DateTime>(), DateTime.MaxValue.Year + 1 }
        };
    }

    public static TheoryData<DateTime, IsoMonthOfYear, DateTime> GetSetMonthData(IFixture fixture)
    {
        return new TheoryData<DateTime, IsoMonthOfYear, DateTime>
        {
            { new DateTime( 2021, 8, 26 ), IsoMonthOfYear.August, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 8, 26 ), IsoMonthOfYear.September, new DateTime( 2021, 9, 26 ) },
            { new DateTime( 2021, 8, 26 ), IsoMonthOfYear.July, new DateTime( 2021, 7, 26 ) },
            { new DateTime( 2021, 8, 31 ), IsoMonthOfYear.April, new DateTime( 2021, 4, 30 ) },
            { new DateTime( 2021, 8, 31 ), IsoMonthOfYear.February, new DateTime( 2021, 2, 28 ) },
            { new DateTime( 2020, 8, 31 ), IsoMonthOfYear.February, new DateTime( 2020, 2, 29 ) },
            { new DateTime( 2021, 6, 26 ), IsoMonthOfYear.August, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 6, 25 ), IsoMonthOfYear.August, new DateTime( 2021, 8, 25 ) }
        };
    }

    public static TheoryData<DateTime, int, DateTime> GetSetDayOfMonthData(IFixture fixture)
    {
        return new TheoryData<DateTime, int, DateTime>
        {
            { new DateTime( 2021, 8, 26 ), 26, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 8, 26 ), 1, new DateTime( 2021, 8, 1 ) },
            { new DateTime( 2021, 8, 26 ), 31, new DateTime( 2021, 8, 31 ) },
            { new DateTime( 2021, 8, 16 ), 26, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 8, 16 ), 25, new DateTime( 2021, 8, 25 ) }
        };
    }

    public static TheoryData<DateTime, int> GetSetDayOfMonthThrowData(IFixture fixture)
    {
        return new TheoryData<DateTime, int>
        {
            { new DateTime( 2021, 1, 1 ), 0 },
            { new DateTime( 2021, 2, 1 ), 0 },
            { new DateTime( 2021, 3, 1 ), 0 },
            { new DateTime( 2021, 4, 1 ), 0 },
            { new DateTime( 2021, 5, 1 ), 0 },
            { new DateTime( 2021, 6, 1 ), 0 },
            { new DateTime( 2021, 7, 1 ), 0 },
            { new DateTime( 2021, 8, 1 ), 0 },
            { new DateTime( 2021, 9, 1 ), 0 },
            { new DateTime( 2021, 10, 1 ), 0 },
            { new DateTime( 2021, 11, 1 ), 0 },
            { new DateTime( 2021, 12, 1 ), 0 },
            { new DateTime( 2021, 1, 1 ), ChronoConstants.DaysInJanuary + 1 },
            { new DateTime( 2021, 2, 1 ), ChronoConstants.DaysInFebruary + 1 },
            { new DateTime( 2020, 2, 1 ), ChronoConstants.DaysInLeapFebruary + 1 },
            { new DateTime( 2021, 3, 1 ), ChronoConstants.DaysInMarch + 1 },
            { new DateTime( 2021, 4, 1 ), ChronoConstants.DaysInApril + 1 },
            { new DateTime( 2021, 5, 1 ), ChronoConstants.DaysInMay + 1 },
            { new DateTime( 2021, 6, 1 ), ChronoConstants.DaysInJune + 1 },
            { new DateTime( 2021, 7, 1 ), ChronoConstants.DaysInJuly + 1 },
            { new DateTime( 2021, 8, 1 ), ChronoConstants.DaysInAugust + 1 },
            { new DateTime( 2021, 9, 1 ), ChronoConstants.DaysInSeptember + 1 },
            { new DateTime( 2021, 10, 1 ), ChronoConstants.DaysInOctober + 1 },
            { new DateTime( 2021, 11, 1 ), ChronoConstants.DaysInNovember + 1 },
            { new DateTime( 2021, 12, 1 ), ChronoConstants.DaysInDecember + 1 },
        };
    }

    public static TheoryData<DateTime, int, DateTime> GetSetDayOfYearData(IFixture fixture)
    {
        return new TheoryData<DateTime, int, DateTime>
        {
            { new DateTime( 2021, 8, 26 ), 1, new DateTime( 2021, 1, 1 ) },
            { new DateTime( 2021, 8, 26 ), 365, new DateTime( 2021, 12, 31 ) },
            { new DateTime( 2020, 8, 26 ), 365, new DateTime( 2020, 12, 30 ) },
            { new DateTime( 2020, 8, 26 ), 366, new DateTime( 2020, 12, 31 ) },
            { new DateTime( 2021, 8, 26 ), 238, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2020, 8, 26 ), 239, new DateTime( 2020, 8, 26 ) },
            { new DateTime( 2021, 6, 16 ), 238, new DateTime( 2021, 8, 26 ) },
            { new DateTime( 2021, 6, 16 ), 237, new DateTime( 2021, 8, 25 ) }
        };
    }

    public static TheoryData<DateTime, int> GetSetDayOfYearThrowData(IFixture fixture)
    {
        return new TheoryData<DateTime, int>
        {
            { new DateTime( 2021, 1, 1 ), 0 },
            { new DateTime( 2021, 1, 1 ), ChronoConstants.DaysInYear + 1 },
            { new DateTime( 2020, 1, 1 ), ChronoConstants.DaysInLeapYear + 1 }
        };
    }

    public static TheoryData<DateTime, TimeOfDay, DateTime> GetSetTimeOfDayData(IFixture fixture)
    {
        var dt = new DateTime( 2021, 8, 26, 17, 18, 19, 234 ).AddTicks( 5678 );

        return new TheoryData<DateTime, TimeOfDay, DateTime>
        {
            { dt, TimeOfDay.Start, new DateTime( 2021, 8, 26 ) },
            { dt, TimeOfDay.Mid, new DateTime( 2021, 8, 26, 12, 0, 0 ) },
            { dt, TimeOfDay.End, new DateTime( 2021, 8, 26, 23, 59, 59, 999 ).AddTicks( 9999 ) },
            {
                dt,
                new TimeOfDay( 12, 30, 40, 500, 6001 ),
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 )
            },
            {
                dt,
                new TimeOfDay( 11, 59, 59, 999, 9999 ),
                new DateTime( 2021, 8, 26, 11, 59, 59, 999 ).AddTicks( 9999 )
            },
            {
                dt,
                new TimeOfDay( 13 ),
                new DateTime( 2021, 8, 26, 13, 0, 0 )
            }
        };
    }

    public static TheoryData<DateTime, DateTime, PeriodUnits, Period> GetGetPeriodOffsetData(
        IFixture fixture)
    {
        return new TheoryData<DateTime, DateTime, PeriodUnits, Period>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                PeriodUnits.All,
                Period.Empty
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 6, 1, 7, 24, 33, 492 ).AddTicks( 5992 ),
                PeriodUnits.All,
                new Period( 1, 2, 3, 4, 5, 6, 7, 8, 9 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.All,
                new Period( 0, 10, 0, 5, 21, 0, 50, 49, 9900 )
            },
            {
                new DateTime( 2022, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 9, 28, 13, 31, 41, 501 ).AddTicks( 6002 ),
                PeriodUnits.All,
                new Period( 1, 10, 3, 6, 22, 58, 58, 998, 9999 )
            },
            {
                new DateTime( 2022, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Years,
                Period.FromYears( 1 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Months,
                Period.FromMonths( 10 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Weeks,
                Period.FromWeeks( 44 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Days,
                Period.FromDays( 309 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Hours,
                Period.FromHours( 7437 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Minutes,
                Period.FromMinutes( 446220 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Seconds,
                Period.FromSeconds( 26773250 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Milliseconds,
                Period.FromMilliseconds( 26773250049 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Ticks,
                Period.FromTicks( 267732500499900 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Date,
                new Period( 0, 10, 0, 5 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Time,
                new Period( 7437, 0, 50, 49, 9900 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Months | PeriodUnits.Days | PeriodUnits.Seconds | PeriodUnits.Ticks,
                Period.FromMonths( 10 ).AddDays( 5 ).AddSeconds( 75650 ).AddTicks( 499900 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2021, 6, 28, 13, 31, 42, 503 ).AddTicks( 6005 ),
                PeriodUnits.Months | PeriodUnits.Weeks | PeriodUnits.Hours,
                Period.FromMonths( 1 ).AddWeeks( 3 ).AddHours( 166 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 6, 26, 11, 31, 42, 503 ).AddTicks( 6005 ),
                PeriodUnits.Months | PeriodUnits.Days | PeriodUnits.Minutes,
                Period.FromMonths( 14 ).AddMinutes( 58 )
            }
        };
    }

    public static TheoryData<DateTime, DateTime, PeriodUnits, Period> GetGetGreedyPeriodOffsetData(
        IFixture fixture)
    {
        return new TheoryData<DateTime, DateTime, PeriodUnits, Period>
        {
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                PeriodUnits.All,
                Period.Empty
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 6, 1, 7, 24, 33, 492 ).AddTicks( 5992 ),
                PeriodUnits.All,
                new Period( 1, 2, 3, 4, 5, 6, 7, 8, 9 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.All,
                new Period( 1, -2, 0, 6, -3, 1, -10, 50, -100 )
            },
            {
                new DateTime( 2022, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 9, 28, 13, 31, 41, 501 ).AddTicks( 6002 ),
                PeriodUnits.All,
                new Period( 2, -1, 0, -2, -1, -1, -1, -1, -1 )
            },
            {
                new DateTime( 2022, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Years,
                Period.FromYears( 2 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Months,
                Period.FromMonths( 10 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Weeks,
                Period.FromWeeks( 44 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Days,
                Period.FromDays( 310 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Hours,
                Period.FromHours( 7437 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Minutes,
                Period.FromMinutes( 446221 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Seconds,
                Period.FromSeconds( 26773250 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Milliseconds,
                Period.FromMilliseconds( 26773250050 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Ticks,
                Period.FromTicks( 267732500499900 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Date,
                new Period( 1, -2, 0, 6 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Time,
                new Period( 7437, 1, -10, 50, -100 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 10, 20, 15, 29, 50, 450 ).AddTicks( 6101 ),
                PeriodUnits.Months | PeriodUnits.Days | PeriodUnits.Seconds | PeriodUnits.Ticks,
                Period.FromMonths( 10 ).AddDays( 6 ).SubtractSeconds( 10750 ).AddTicks( 499900 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2021, 6, 28, 13, 31, 42, 503 ).AddTicks( 6005 ),
                PeriodUnits.Months | PeriodUnits.Weeks | PeriodUnits.Hours,
                Period.FromMonths( 2 ).SubtractHours( 49 )
            },
            {
                new DateTime( 2021, 8, 26, 12, 30, 40, 500 ).AddTicks( 6001 ),
                new DateTime( 2020, 6, 26, 11, 31, 42, 503 ).AddTicks( 6005 ),
                PeriodUnits.Months | PeriodUnits.Days | PeriodUnits.Minutes,
                Period.FromMonths( 14 ).AddMinutes( 59 )
            }
        };
    }
}
