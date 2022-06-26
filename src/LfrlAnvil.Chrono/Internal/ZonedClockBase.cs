using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono.Internal;

public abstract class ZonedClockBase : IZonedClock
{
    protected ZonedClockBase(TimeZoneInfo timeZone)
    {
        TimeZone = timeZone;
    }

    public TimeZoneInfo TimeZone { get; }

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
