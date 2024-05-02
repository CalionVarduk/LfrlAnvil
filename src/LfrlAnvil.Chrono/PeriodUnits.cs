using System;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents available <see cref="Period"/> date and/or time units.
/// </summary>
[Flags]
public enum PeriodUnits : ushort
{
    /// <summary>
    /// Represents a lack of period unit.
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents ticks time unit.
    /// </summary>
    Ticks = 1,

    /// <summary>
    /// Represents milliseconds time unit.
    /// </summary>
    Milliseconds = 2,

    /// <summary>
    /// Represents microseconds time unit.
    /// </summary>
    Microseconds = 4,

    /// <summary>
    /// Represents seconds time unit.
    /// </summary>
    Seconds = 8,

    /// <summary>
    /// Represents minutes time unit.
    /// </summary>
    Minutes = 16,

    /// <summary>
    /// Represents hours time unit.
    /// </summary>
    Hours = 32,

    /// <summary>
    /// Represents days date unit.
    /// </summary>
    Days = 64,

    /// <summary>
    /// Represents weeks date unit.
    /// </summary>
    Weeks = 128,

    /// <summary>
    /// Represents months date unit.
    /// </summary>
    Months = 256,

    /// <summary>
    /// Represents years date unit.
    /// </summary>
    Years = 512,

    /// <summary>
    /// Represents all available date units.
    /// </summary>
    Date = Days | Weeks | Months | Years,

    /// <summary>
    /// Represents all available time units.
    /// </summary>
    Time = Ticks | Microseconds | Milliseconds | Seconds | Minutes | Hours,

    /// <summary>
    /// Represents all available units.
    /// </summary>
    All = Date | Time
}
