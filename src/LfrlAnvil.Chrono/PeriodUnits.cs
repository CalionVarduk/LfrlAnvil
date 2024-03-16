using System;

namespace LfrlAnvil.Chrono;

[Flags]
public enum PeriodUnits : ushort
{
    None = 0,
    Ticks = 1,
    Milliseconds = 2,
    Microseconds = 4,
    Seconds = 8,
    Minutes = 16,
    Hours = 32,
    Days = 64,
    Weeks = 128,
    Months = 256,
    Years = 512,
    Date = Days | Weeks | Months | Years,
    Time = Ticks | Microseconds | Milliseconds | Seconds | Minutes | Hours,
    All = Date | Time
}
