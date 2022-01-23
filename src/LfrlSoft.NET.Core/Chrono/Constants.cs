using System;

namespace LfrlSoft.NET.Core.Chrono
{
    public static class Constants
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
    }
}
