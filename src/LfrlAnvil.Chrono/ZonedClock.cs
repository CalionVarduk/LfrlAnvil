using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono
{
    public sealed class ZonedClock : ZonedClockBase
    {
        public static readonly ZonedClock Utc = new ZonedClock( TimeZoneInfo.Utc );
        public static readonly ZonedClock Local = new ZonedClock( TimeZoneInfo.Local );

        public ZonedClock(TimeZoneInfo timeZone)
            : base( timeZone ) { }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override ZonedDateTime GetNow()
        {
            return ZonedDateTime.CreateUtc( DateTime.UtcNow ).ToTimeZone( TimeZone );
        }
    }
}
