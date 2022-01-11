using System;

namespace LfrlSoft.NET.Core.Chrono
{
    public interface IZonedClock
    {
        TimeZoneInfo TimeZone { get; }
        ZonedDateTime GetNow();
    }
}
