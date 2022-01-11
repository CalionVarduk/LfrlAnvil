using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono
{
    public sealed class ZonedClock : IZonedClock
    {
        public static readonly ZonedClock Utc = new ZonedClock( TimeZoneInfo.Utc );
        public static readonly ZonedClock Local = new ZonedClock( TimeZoneInfo.Local );

        public ZonedClock(TimeZoneInfo timeZone)
        {
            TimeZone = timeZone;
        }

        public TimeZoneInfo TimeZone { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ZonedDateTime GetNow()
        {
            return ZonedDateTime.CreateUtc( DateTime.UtcNow ).ToTimeZone( TimeZone );
        }
    }
}
