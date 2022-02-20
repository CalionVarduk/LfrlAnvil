using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono
{
    public sealed class FrozenZonedClock : IZonedClock
    {
        private readonly ZonedDateTime _now;

        public FrozenZonedClock(ZonedDateTime now)
        {
            _now = now;
            TimeZone = now.TimeZone;
        }

        public TimeZoneInfo TimeZone { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime GetNow()
        {
            return _now;
        }
    }
}
