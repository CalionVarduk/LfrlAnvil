using System;

namespace LfrlAnvil.Chrono
{
    public interface IZonedClock
    {
        TimeZoneInfo TimeZone { get; }
        ZonedDateTime GetNow();
    }
}
