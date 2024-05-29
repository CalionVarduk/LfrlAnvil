using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono.Internal;

/// <inheritdoc cref="IZonedClock" />
public abstract class ZonedClockBase : IZonedClock
{
    /// <summary>
    /// Creates a new <see cref="ZonedClockBase"/> instance.
    /// </summary>
    /// <param name="timeZone">Time zone of this clock.</param>
    protected ZonedClockBase(TimeZoneInfo timeZone)
    {
        TimeZone = timeZone;
    }

    /// <inheritdoc />
    public TimeZoneInfo TimeZone { get; }

    /// <inheritdoc />
    public abstract ZonedDateTime GetNow();

    [Pure]
    ZonedDateTime IGenerator<ZonedDateTime>.Generate()
    {
        return GetNow();
    }

    bool IGenerator<ZonedDateTime>.TryGenerate(out ZonedDateTime result)
    {
        result = GetNow();
        return true;
    }

    [Pure]
    object IGenerator.Generate()
    {
        return GetNow();
    }

    bool IGenerator.TryGenerate(out object result)
    {
        result = GetNow();
        return true;
    }
}
