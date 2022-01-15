using System;

namespace LfrlSoft.NET.Core.Chrono
{
    [Flags]
    public enum PeriodUnits
    {
        None = 0,
        Ticks = 1,
        Milliseconds = 2,
        Seconds = 4,
        Minutes = 8,
        Hours = 16,
        Days = 32,
        Weeks = 64,
        Months = 128,
        Years = 256,
        Date = Days | Weeks | Months | Years,
        Time = Ticks | Milliseconds | Seconds | Minutes | Hours,
        All = Date | Time
    }
}
