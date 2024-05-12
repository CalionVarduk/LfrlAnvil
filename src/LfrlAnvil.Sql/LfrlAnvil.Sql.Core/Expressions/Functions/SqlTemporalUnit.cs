namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents available temporal units for date and/or time related functions.
/// </summary>
public enum SqlTemporalUnit : byte
{
    /// <summary>
    /// Specifies a nanosecond time unit.
    /// </summary>
    Nanosecond = 0,

    /// <summary>
    /// Specifies a microsecond time unit.
    /// </summary>
    Microsecond = 1,

    /// <summary>
    /// Specifies a millisecond time unit.
    /// </summary>
    Millisecond = 2,

    /// <summary>
    /// Specifies a second time unit.
    /// </summary>
    Second = 3,

    /// <summary>
    /// Specifies a minute time unit.
    /// </summary>
    Minute = 4,

    /// <summary>
    /// Specifies a hour time unit.
    /// </summary>
    Hour = 5,

    /// <summary>
    /// Specifies a day date unit.
    /// </summary>
    Day = 6,

    /// <summary>
    /// Specifies a week date unit.
    /// </summary>
    Week = 7,

    /// <summary>
    /// Specifies a month date unit.
    /// </summary>
    Month = 8,

    /// <summary>
    /// Specifies a year date unit.
    /// </summary>
    Year = 9
}
