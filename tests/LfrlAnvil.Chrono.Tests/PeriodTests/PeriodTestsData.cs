using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.PeriodTests;

public class PeriodTestsData
{
    public static TheoryData<int, int, int, int, PeriodUnits> GetCtorWithDateData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, PeriodUnits>
        {
            { 1, 0, 0, 0, PeriodUnits.Years },
            { 0, 1, 0, 0, PeriodUnits.Months },
            { 0, 0, 1, 0, PeriodUnits.Weeks },
            { 0, 0, 0, 1, PeriodUnits.Days },
            { 1, 2, 3, 4, PeriodUnits.Date },
            { -1, 0, 0, 0, PeriodUnits.Years },
            { 0, -1, 0, 0, PeriodUnits.Months },
            { 0, 0, -1, 0, PeriodUnits.Weeks },
            { 0, 0, 0, -1, PeriodUnits.Days },
            { -1, -2, -3, -4, PeriodUnits.Date },
            { -1, 2, 3, 4, PeriodUnits.Date },
            { 1, -2, 3, 4, PeriodUnits.Date },
            { 1, 2, -3, 4, PeriodUnits.Date },
            { 1, 2, 3, -4, PeriodUnits.Date },
            { 1, -2, -3, -4, PeriodUnits.Date },
            { -1, 2, -3, -4, PeriodUnits.Date },
            { -1, -2, 3, -4, PeriodUnits.Date },
            { -1, -2, -3, 4, PeriodUnits.Date }
        };
    }

    public static TheoryData<int, int, int, int, int, PeriodUnits> GetCtorWithTimeData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int, int, PeriodUnits>
        {
            { 1, 0, 0, 0, 0, PeriodUnits.Hours },
            { 0, 1, 0, 0, 0, PeriodUnits.Minutes },
            { 0, 0, 1, 0, 0, PeriodUnits.Seconds },
            { 0, 0, 0, 1, 0, PeriodUnits.Milliseconds },
            { 0, 0, 0, 0, 1, PeriodUnits.Ticks },
            { 1, 2, 3, 4, 5, PeriodUnits.Time },
            { -1, 0, 0, 0, 0, PeriodUnits.Hours },
            { 0, -1, 0, 0, 0, PeriodUnits.Minutes },
            { 0, 0, -1, 0, 0, PeriodUnits.Seconds },
            { 0, 0, 0, -1, 0, PeriodUnits.Milliseconds },
            { 0, 0, 0, 0, -1, PeriodUnits.Ticks },
            { -1, -2, -3, -4, -5, PeriodUnits.Time },
            { -1, 2, 3, 4, 5, PeriodUnits.Time },
            { 1, -2, 3, 4, 5, PeriodUnits.Time },
            { 1, 2, -3, 4, 5, PeriodUnits.Time },
            { 1, 2, 3, -4, 5, PeriodUnits.Time },
            { 1, 2, 3, 4, -5, PeriodUnits.Time },
            { 1, -2, -3, -4, -5, PeriodUnits.Time },
            { -1, 2, -3, -4, -5, PeriodUnits.Time },
            { -1, -2, 3, -4, -5, PeriodUnits.Time },
            { -1, -2, -3, 4, -5, PeriodUnits.Time },
            { -1, -2, -3, -4, 5, PeriodUnits.Time }
        };
    }

    public static TheoryData<
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            PeriodUnits>
        GetCtorWithFullData(IFixture fixture)
    {
        return new TheoryData<(int, int, int, int), (int, int, int, int, int), PeriodUnits>
        {
            { (1, 0, 0, 0), (0, 0, 0, 0, 0), PeriodUnits.Years },
            { (0, 1, 0, 0), (0, 0, 0, 0, 0), PeriodUnits.Months },
            { (0, 0, 1, 0), (0, 0, 0, 0, 0), PeriodUnits.Weeks },
            { (0, 0, 0, 1), (0, 0, 0, 0, 0), PeriodUnits.Days },
            { (0, 0, 0, 0), (1, 0, 0, 0, 0), PeriodUnits.Hours },
            { (0, 0, 0, 0), (0, 1, 0, 0, 0), PeriodUnits.Minutes },
            { (0, 0, 0, 0), (0, 0, 1, 0, 0), PeriodUnits.Seconds },
            { (0, 0, 0, 0), (0, 0, 0, 1, 0), PeriodUnits.Milliseconds },
            { (0, 0, 0, 0), (0, 0, 0, 0, 1), PeriodUnits.Ticks },
            { (1, 2, 3, 4), (0, 0, 0, 0, 0), PeriodUnits.Date },
            { (0, 0, 0, 0), (1, 2, 3, 4, 5), PeriodUnits.Time },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), PeriodUnits.All },
            { (-1, -2, -3, -4), (0, 0, 0, 0, 0), PeriodUnits.Date },
            { (0, 0, 0, 0), (-1, -2, -3, -4, -5), PeriodUnits.Time },
            { (-1, -2, -3, -4), (-5, -6, -7, -8, -9), PeriodUnits.All },
            { (-1, 2, 3, 4), (5, 6, 7, 8, 9), PeriodUnits.All },
            { (-1, 2, -3, -4), (-5, -6, -7, -8, -9), PeriodUnits.All },
            { (1, 2, -3, 4), (5, 6, 7, 8, 9), PeriodUnits.All },
            { (-1, -2, -3, 4), (-5, -6, -7, -8, -9), PeriodUnits.All },
            { (1, 2, 3, 4), (-5, 6, 7, 8, 9), PeriodUnits.All },
            { (-1, -2, -3, -4), (-5, 6, -7, -8, -9), PeriodUnits.All },
            { (1, 2, 3, 4), (5, 6, -7, 8, 9), PeriodUnits.All },
            { (-1, -2, -3, -4), (-5, -6, -7, 8, -9), PeriodUnits.All },
            { (1, 2, 3, 4), (5, 6, 7, 8, -9), PeriodUnits.All }
        };
    }

    public static TheoryData<TimeSpan, int, int, int, int, int, int, PeriodUnits> GetCtorWithTimeSpanData(IFixture fixture)
    {
        var day = TimeSpan.FromDays( 1 );
        var hour = TimeSpan.FromHours( 1 );
        var minute = TimeSpan.FromMinutes( 1 );
        var second = TimeSpan.FromSeconds( 1 );
        var millisecond = TimeSpan.FromMilliseconds( 1 );
        var tick = TimeSpan.FromTicks( 1 );

        var allUnits = PeriodUnits.Days | PeriodUnits.Time;

        return new TheoryData<TimeSpan, int, int, int, int, int, int, PeriodUnits>
        {
            { TimeSpan.Zero, 0, 0, 0, 0, 0, 0, PeriodUnits.None },
            { day, 1, 0, 0, 0, 0, 0, PeriodUnits.Days },
            { hour, 0, 1, 0, 0, 0, 0, PeriodUnits.Hours },
            { minute, 0, 0, 1, 0, 0, 0, PeriodUnits.Minutes },
            { second, 0, 0, 0, 1, 0, 0, PeriodUnits.Seconds },
            { millisecond, 0, 0, 0, 0, 1, 0, PeriodUnits.Milliseconds },
            { tick, 0, 0, 0, 0, 0, 1, PeriodUnits.Ticks },
            { -day, -1, 0, 0, 0, 0, 0, PeriodUnits.Days },
            { -hour, 0, -1, 0, 0, 0, 0, PeriodUnits.Hours },
            { -minute, 0, 0, -1, 0, 0, 0, PeriodUnits.Minutes },
            { -second, 0, 0, 0, -1, 0, 0, PeriodUnits.Seconds },
            { -millisecond, 0, 0, 0, 0, -1, 0, PeriodUnits.Milliseconds },
            { -tick, 0, 0, 0, 0, 0, -1, PeriodUnits.Ticks },
            { day + hour * 2 + minute * 3 + second * 4 + millisecond * 5 + tick * 6, 1, 2, 3, 4, 5, 6, allUnits },
            { -day - hour * 2 - minute * 3 - second * 4 - millisecond * 5 - tick * 6, -1, -2, -3, -4, -5, -6, allUnits }
        };
    }

    public static TheoryData<
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            string>
        GetToStringData(IFixture fixture)
    {
        return new TheoryData<(int, int, int, int), (int, int, int, int, int), string>
        {
            { (0, 0, 0, 0), (0, 0, 0, 0, 0), "0 day(s)" },
            { (1, 0, 0, 0), (0, 0, 0, 0, 0), "1 year(s)" },
            { (0, 1, 0, 0), (0, 0, 0, 0, 0), "1 month(s)" },
            { (0, 0, 1, 0), (0, 0, 0, 0, 0), "1 week(s)" },
            { (0, 0, 0, 1), (0, 0, 0, 0, 0), "1 day(s)" },
            { (0, 0, 0, 0), (1, 0, 0, 0, 0), "1 hour(s)" },
            { (0, 0, 0, 0), (0, 1, 0, 0, 0), "1 minute(s)" },
            { (0, 0, 0, 0), (0, 0, 1, 0, 0), "1 second(s)" },
            { (0, 0, 0, 0), (0, 0, 0, 1, 0), "1 millisecond(s)" },
            { (0, 0, 0, 0), (0, 0, 0, 0, 1), "1 tick(s)" },
            { (1, 2, 3, 4), (0, 0, 0, 0, 0), "1 year(s), 2 month(s), 3 week(s), 4 day(s)" },
            { (0, 0, 0, 0), (1, 2, 3, 4, 5), "1 hour(s), 2 minute(s), 3 second(s), 4 millisecond(s), 5 tick(s)" },
            {
                (1, 2, 3, 4),
                (5, 6, 7, 8, 9),
                "1 year(s), 2 month(s), 3 week(s), 4 day(s), 5 hour(s), 6 minute(s), 7 second(s), 8 millisecond(s), 9 tick(s)"
            },
            { (-1, 0, 2, 0), (-3, 0, 4, 0, -5), "-1 year(s), 2 week(s), -3 hour(s), 4 second(s), -5 tick(s)" }
        };
    }

    public static TheoryData<
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            bool>
        GetEqualsData(IFixture fixture)
    {
        return new TheoryData<(int, int, int, int), (int, int, int, int, int), (int, int, int, int), (int, int, int, int, int), bool>
        {
            { (0, 0, 0, 0), (0, 0, 0, 0, 0), (0, 0, 0, 0), (0, 0, 0, 0, 0), true },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), true },
            { (2, 2, 3, 4), (5, 6, 7, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), false },
            { (1, 3, 3, 4), (5, 6, 7, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), false },
            { (1, 2, 4, 4), (5, 6, 7, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), false },
            { (1, 2, 3, 5), (5, 6, 7, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), false },
            { (1, 2, 3, 4), (6, 6, 7, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), false },
            { (1, 2, 3, 4), (5, 7, 7, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), false },
            { (1, 2, 3, 4), (5, 6, 8, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), false },
            { (1, 2, 3, 4), (5, 6, 7, 9, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), false },
            { (1, 2, 3, 4), (5, 6, 7, 8, 10), (1, 2, 3, 4), (5, 6, 7, 8, 9), false }
        };
    }

    public static TheoryData<
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks)>
        GetAddData(IFixture fixture)
    {
        return new TheoryData<
            (int, int, int, int), (int, int, int, int, int),
            (int, int, int, int), (int, int, int, int, int),
            (int, int, int, int), (int, int, int, int, int)>
        {
            { (0, 0, 0, 0), (0, 0, 0, 0, 0), (0, 0, 0, 0), (0, 0, 0, 0, 0), (0, 0, 0, 0), (0, 0, 0, 0, 0) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), (2, 4, 6, 8), (10, 12, 14, 16, 18) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (1, 0, 0, 0), (0, 0, 0, 0, 0), (2, 2, 3, 4), (5, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 1, 0, 0), (0, 0, 0, 0, 0), (1, 3, 3, 4), (5, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 1, 0), (0, 0, 0, 0, 0), (1, 2, 4, 4), (5, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 1), (0, 0, 0, 0, 0), (1, 2, 3, 5), (5, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (1, 0, 0, 0, 0), (1, 2, 3, 4), (6, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (0, 1, 0, 0, 0), (1, 2, 3, 4), (5, 7, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (0, 0, 1, 0, 0), (1, 2, 3, 4), (5, 6, 8, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (0, 0, 0, 1, 0), (1, 2, 3, 4), (5, 6, 7, 9, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (0, 0, 0, 0, 1), (1, 2, 3, 4), (5, 6, 7, 8, 10) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (-1, -2, -3, -4), (-5, -6, -7, -8, -9), (0, 0, 0, 0), (0, 0, 0, 0, 0) }
        };
    }

    public static TheoryData<
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks)>
        GetSubtractData(IFixture fixture)
    {
        return new TheoryData<
            (int, int, int, int), (int, int, int, int, int),
            (int, int, int, int), (int, int, int, int, int),
            (int, int, int, int), (int, int, int, int, int)>
        {
            { (0, 0, 0, 0), (0, 0, 0, 0, 0), (0, 0, 0, 0), (0, 0, 0, 0, 0), (0, 0, 0, 0), (0, 0, 0, 0, 0) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (0, 0, 0, 0, 0) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (1, 0, 0, 0), (0, 0, 0, 0, 0), (0, 2, 3, 4), (5, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 1, 0, 0), (0, 0, 0, 0, 0), (1, 1, 3, 4), (5, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 1, 0), (0, 0, 0, 0, 0), (1, 2, 2, 4), (5, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 1), (0, 0, 0, 0, 0), (1, 2, 3, 3), (5, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (1, 0, 0, 0, 0), (1, 2, 3, 4), (4, 6, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (0, 1, 0, 0, 0), (1, 2, 3, 4), (5, 5, 7, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (0, 0, 1, 0, 0), (1, 2, 3, 4), (5, 6, 6, 8, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (0, 0, 0, 1, 0), (1, 2, 3, 4), (5, 6, 7, 7, 9) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (0, 0, 0, 0), (0, 0, 0, 0, 1), (1, 2, 3, 4), (5, 6, 7, 8, 8) },
            { (1, 2, 3, 4), (5, 6, 7, 8, 9), (-1, -2, -3, -4), (-5, -6, -7, -8, -9), (2, 4, 6, 8), (10, 12, 14, 16, 18) }
        };
    }

    public static TheoryData<
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            PeriodUnits,
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks)>
        GetSetData(IFixture fixture)
    {
        var date1 = (1, 2, 3, 4);
        var time1 = (5, 6, 7, 8, 9);
        var date2 = (10, 20, 30, 40);
        var time2 = (50, 60, 70, 80, 90);

        return new TheoryData<
            (int, int, int, int), (int, int, int, int, int),
            (int, int, int, int), (int, int, int, int, int),
            PeriodUnits,
            (int, int, int, int), (int, int, int, int, int)>
        {
            { date1, time1, date2, time2, PeriodUnits.None, date1, time1 },
            { date1, time1, date2, time2, PeriodUnits.All, date2, time2 },
            { date1, time1, date2, time2, PeriodUnits.Date, date2, time1 },
            { date1, time1, date2, time2, PeriodUnits.Time, date1, time2 },
            { date1, time1, date2, time2, PeriodUnits.Ticks, date1, (5, 6, 7, 8, 90) },
            { date1, time1, date2, time2, PeriodUnits.Milliseconds, date1, (5, 6, 7, 80, 9) },
            { date1, time1, date2, time2, PeriodUnits.Seconds, date1, (5, 6, 70, 8, 9) },
            { date1, time1, date2, time2, PeriodUnits.Minutes, date1, (5, 60, 7, 8, 9) },
            { date1, time1, date2, time2, PeriodUnits.Hours, date1, (50, 6, 7, 8, 9) },
            { date1, time1, date2, time2, PeriodUnits.Days, (1, 2, 3, 40), time1 },
            { date1, time1, date2, time2, PeriodUnits.Weeks, (1, 2, 30, 4), time1 },
            { date1, time1, date2, time2, PeriodUnits.Months, (1, 20, 3, 4), time1 },
            { date1, time1, date2, time2, PeriodUnits.Years, (10, 2, 3, 4), time1 }
        };
    }

    public static TheoryData<
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks)>
        GetAbsData(IFixture fixture)
    {
        var date = (1, 2, 3, 4);
        var time = (5, 6, 7, 8, 9);
        var negatedDate = (-1, -2, -3, -4);
        var negatedTime = (-5, -6, -7, -8, -9);

        return new TheoryData<
            (int, int, int, int), (int, int, int, int, int),
            (int, int, int, int), (int, int, int, int, int)>
        {
            { date, time, date, time },
            { negatedDate, time, date, time },
            { date, negatedTime, date, time },
            { negatedDate, negatedTime, date, time }
        };
    }

    public static TheoryData<
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            PeriodUnits,
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks)>
        GetSkipData(IFixture fixture)
    {
        var date = (1, 2, 3, 4);
        var time = (5, 6, 7, 8, 9);

        return new TheoryData<
            (int, int, int, int), (int, int, int, int, int),
            PeriodUnits,
            (int, int, int, int), (int, int, int, int, int)>
        {
            { date, time, PeriodUnits.None, date, time },
            { date, time, PeriodUnits.All, (0, 0, 0, 0), (0, 0, 0, 0, 0) },
            { date, time, PeriodUnits.Date, (0, 0, 0, 0), time },
            { date, time, PeriodUnits.Time, date, (0, 0, 0, 0, 0) },
            { date, time, PeriodUnits.Ticks, date, (5, 6, 7, 8, 0) },
            { date, time, PeriodUnits.Milliseconds, date, (5, 6, 7, 0, 9) },
            { date, time, PeriodUnits.Seconds, date, (5, 6, 0, 8, 9) },
            { date, time, PeriodUnits.Minutes, date, (5, 0, 7, 8, 9) },
            { date, time, PeriodUnits.Hours, date, (0, 6, 7, 8, 9) },
            { date, time, PeriodUnits.Days, (1, 2, 3, 0), time },
            { date, time, PeriodUnits.Weeks, (1, 2, 0, 4), time },
            { date, time, PeriodUnits.Months, (1, 0, 3, 4), time },
            { date, time, PeriodUnits.Years, (0, 2, 3, 4), time }
        };
    }

    public static TheoryData<
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks),
            PeriodUnits,
            (int Years, int Months, int Weeks, int Days),
            (int Hours, int Minutes, int Seconds, int Milliseconds, int Ticks)>
        GetTakeData(IFixture fixture)
    {
        var date = (1, 2, 3, 4);
        var time = (5, 6, 7, 8, 9);

        return new TheoryData<
            (int, int, int, int), (int, int, int, int, int),
            PeriodUnits,
            (int, int, int, int), (int, int, int, int, int)>
        {
            { date, time, PeriodUnits.None, (0, 0, 0, 0), (0, 0, 0, 0, 0) },
            { date, time, PeriodUnits.All, date, time },
            { date, time, PeriodUnits.Date, date, (0, 0, 0, 0, 0) },
            { date, time, PeriodUnits.Time, (0, 0, 0, 0), time },
            { date, time, PeriodUnits.Ticks, (0, 0, 0, 0), (0, 0, 0, 0, 9) },
            { date, time, PeriodUnits.Milliseconds, (0, 0, 0, 0), (0, 0, 0, 8, 0) },
            { date, time, PeriodUnits.Seconds, (0, 0, 0, 0), (0, 0, 7, 0, 0) },
            { date, time, PeriodUnits.Minutes, (0, 0, 0, 0), (0, 6, 0, 0, 0) },
            { date, time, PeriodUnits.Hours, (0, 0, 0, 0), (5, 0, 0, 0, 0) },
            { date, time, PeriodUnits.Days, (0, 0, 0, 4), (0, 0, 0, 0, 0) },
            { date, time, PeriodUnits.Weeks, (0, 0, 3, 0), (0, 0, 0, 0, 0) },
            { date, time, PeriodUnits.Months, (0, 2, 0, 0), (0, 0, 0, 0, 0) },
            { date, time, PeriodUnits.Years, (1, 0, 0, 0), (0, 0, 0, 0, 0) }
        };
    }

    public static IEnumerable<object?[]> GetNotEqualsData(IFixture fixture)
    {
        return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
    }
}
