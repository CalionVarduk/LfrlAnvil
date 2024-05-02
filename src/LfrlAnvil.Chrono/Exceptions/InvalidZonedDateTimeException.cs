using System;
using LfrlAnvil.Chrono.Extensions;

namespace LfrlAnvil.Chrono.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid <see cref="DateTime"/> within a chosen <see cref="TimeZoneInfo"/>.
/// </summary>
public class InvalidZonedDateTimeException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidZonedDateTimeException"/> instance.
    /// </summary>
    /// <param name="dateTime">Invalid date time.</param>
    /// <param name="timeZone">Target time zone.</param>
    public InvalidZonedDateTimeException(DateTime dateTime, TimeZoneInfo timeZone)
        : base( CreateMessage( dateTime, timeZone ) )
    {
        DateTime = dateTime;
        TimeZone = timeZone;
    }

    /// <summary>
    /// Invalid date time.
    /// </summary>
    public DateTime DateTime { get; }

    /// <summary>
    /// Target time zone.
    /// </summary>
    public TimeZoneInfo TimeZone { get; }

    private static string CreateMessage(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var invalidityRange = timeZone.GetContainingInvalidityRange( dateTime );
        return invalidityRange is null
            ? Resources.InvalidDateTimeInTimeZone( dateTime, timeZone )
            : Resources.InvalidDateTimeInTimeZoneBecauseOfRange( dateTime, timeZone, invalidityRange.Value );
    }
}
