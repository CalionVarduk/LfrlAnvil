using System;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono
{
    public interface IZonedClock : IGenerator<ZonedDateTime>
    {
        TimeZoneInfo TimeZone { get; }
        ZonedDateTime GetNow();
    }
}
