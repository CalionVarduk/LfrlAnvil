using System;

namespace LfrlAnvil.Chrono;

public static class ChronoConstants
{
    public const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
    public const long TicksPerSecond = TimeSpan.TicksPerSecond;
    public const long TicksPerMinute = TimeSpan.TicksPerMinute;
    public const long TicksPerHour = TimeSpan.TicksPerHour;
    public const long TicksPerDay = TimeSpan.TicksPerDay;
    public const long TicksPerWeek = TicksPerDay * DaysPerWeek;
    public const int MillisecondsPerSecond = 1000;
    public const int SecondsPerMinute = 60;
    public const int MinutesPerHour = 60;
    public const int HoursPerDay = 24;
    public const int DaysPerWeek = 7;
    public const int MonthsPerYear = 12;
    public const int DaysInJanuary = 31;
    public const int DaysInFebruary = 28;
    public const int DaysInLeapFebruary = DaysInFebruary + 1;
    public const int DaysInMarch = 31;
    public const int DaysInApril = 30;
    public const int DaysInMay = 31;
    public const int DaysInJune = 30;
    public const int DaysInJuly = 31;
    public const int DaysInAugust = 31;
    public const int DaysInSeptember = 30;
    public const int DaysInOctober = 31;
    public const int DaysInNovember = 30;
    public const int DaysInDecember = 31;

    public const int DaysInYear = DaysInJanuary +
        DaysInFebruary +
        DaysInMarch +
        DaysInApril +
        DaysInMay +
        DaysInJune +
        DaysInJuly +
        DaysInAugust +
        DaysInSeptember +
        DaysInOctober +
        DaysInNovember +
        DaysInDecember;

    public const int DaysInLeapYear = DaysInYear - DaysInFebruary + DaysInLeapFebruary;
}
