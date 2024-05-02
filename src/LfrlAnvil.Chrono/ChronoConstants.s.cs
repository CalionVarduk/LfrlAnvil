using System;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Contains various time related constant values.
/// </summary>
public static class ChronoConstants
{
    /// <summary>
    /// Specifies the number of ticks in a single microsecond.
    /// </summary>
    public const long TicksPerMicrosecond = TimeSpan.TicksPerMicrosecond;

    /// <summary>
    /// Specifies the number of ticks in a single millisecond.
    /// </summary>
    public const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;

    /// <summary>
    /// Specifies the number of ticks in a single second.
    /// </summary>
    public const long TicksPerSecond = TimeSpan.TicksPerSecond;

    /// <summary>
    /// Specifies the number of ticks in a single minute.
    /// </summary>
    public const long TicksPerMinute = TimeSpan.TicksPerMinute;

    /// <summary>
    /// Specifies the number of ticks in a single hour.
    /// </summary>
    public const long TicksPerHour = TimeSpan.TicksPerHour;

    /// <summary>
    /// Specifies the number of ticks in a single standard (24 hours) day.
    /// </summary>
    public const long TicksPerStandardDay = TimeSpan.TicksPerDay;

    /// <summary>
    /// Specifies the number of ticks in a single week with standard days.
    /// </summary>
    public const long TicksPerStandardWeek = TicksPerStandardDay * DaysPerWeek;

    /// <summary>
    /// Specifies the number of microseconds in a single millisecond.
    /// </summary>
    public const int MicrosecondsPerMillisecond = 1000;

    /// <summary>
    /// Specifies the number of milliseconds in a single second.
    /// </summary>
    public const int MillisecondsPerSecond = 1000;

    /// <summary>
    /// Specifies the number of seconds in a single minute.
    /// </summary>
    public const int SecondsPerMinute = 60;

    /// <summary>
    /// Specifies the number of minutes in a single hour.
    /// </summary>
    public const int MinutesPerHour = 60;

    /// <summary>
    /// Specifies the number of hours in a single standard day.
    /// </summary>
    public const int HoursPerStandardDay = 24;

    /// <summary>
    /// Specifies the number of days in a single week.
    /// </summary>
    public const int DaysPerWeek = 7;

    /// <summary>
    /// Specifies the number of months in a single year.
    /// </summary>
    public const int MonthsPerYear = 12;

    /// <summary>
    /// Specifies the number of days in january.
    /// </summary>
    public const int DaysInJanuary = 31;

    /// <summary>
    /// Specifies the number of days in february in a non-leap year.
    /// </summary>
    public const int DaysInFebruary = 28;

    /// <summary>
    /// Specifies the number of days in february in a leap year.
    /// </summary>
    public const int DaysInLeapFebruary = DaysInFebruary + 1;

    /// <summary>
    /// Specifies the number of days in march.
    /// </summary>
    public const int DaysInMarch = 31;

    /// <summary>
    /// Specifies the number of days in april.
    /// </summary>
    public const int DaysInApril = 30;

    /// <summary>
    /// Specifies the number of days in may.
    /// </summary>
    public const int DaysInMay = 31;

    /// <summary>
    /// Specifies the number of days in june.
    /// </summary>
    public const int DaysInJune = 30;

    /// <summary>
    /// Specifies the number of days in july.
    /// </summary>
    public const int DaysInJuly = 31;

    /// <summary>
    /// Specifies the number of days in august.
    /// </summary>
    public const int DaysInAugust = 31;

    /// <summary>
    /// Specifies the number of days in september.
    /// </summary>
    public const int DaysInSeptember = 30;

    /// <summary>
    /// Specifies the number of days in october.
    /// </summary>
    public const int DaysInOctober = 31;

    /// <summary>
    /// Specifies the number of days in november.
    /// </summary>
    public const int DaysInNovember = 30;

    /// <summary>
    /// Specifies the number of days in december.
    /// </summary>
    public const int DaysInDecember = 31;

    /// <summary>
    /// Specifies the number of days in a single non-leap year.
    /// </summary>
    public const int DaysInYear = DaysInJanuary
        + DaysInFebruary
        + DaysInMarch
        + DaysInApril
        + DaysInMay
        + DaysInJune
        + DaysInJuly
        + DaysInAugust
        + DaysInSeptember
        + DaysInOctober
        + DaysInNovember
        + DaysInDecember;

    /// <summary>
    /// Specifies the number of days in a single leap year.
    /// </summary>
    public const int DaysInLeapYear = DaysInYear - DaysInFebruary + DaysInLeapFebruary;
}
