using System;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a provider of <see cref="ZonedDateTime"/> instances.
/// </summary>
public interface IZonedClock : IGenerator<ZonedDateTime>
{
    /// <summary>
    /// Specifies the time zone of this clock.
    /// </summary>
    TimeZoneInfo TimeZone { get; }

    /// <summary>
    /// Returns the current <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <returns>Current <see cref="ZonedDateTime"/>.</returns>
    ZonedDateTime GetNow();
}
