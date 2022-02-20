using System;
using System.Diagnostics;

namespace LfrlAnvil.Chrono
{
    public sealed class PreciseZonedClock : IZonedClock
    {
        public static readonly PreciseZonedClock Utc = new PreciseZonedClock( TimeZoneInfo.Utc );
        public static readonly PreciseZonedClock Local = new PreciseZonedClock( TimeZoneInfo.Local );

        private DateTime _utcStart = DateTime.UtcNow;
        private double _startTimestamp = Stopwatch.GetTimestamp();

        public PreciseZonedClock(TimeZoneInfo timeZone)
            : this( timeZone, ChronoConstants.TicksPerSecond ) { }

        public PreciseZonedClock(TimeZoneInfo timeZone, long maxIdleTimeInTicks)
        {
            Ensure.IsGreaterThan( maxIdleTimeInTicks, 0, nameof( maxIdleTimeInTicks ) );
            TimeZone = timeZone;
            MaxIdleTimeInTicks = maxIdleTimeInTicks;
        }

        public TimeZoneInfo TimeZone { get; }
        public double MaxIdleTimeInTicks { get; }

        public ZonedDateTime GetNow()
        {
            var endTimestamp = Stopwatch.GetTimestamp();
            var idleTimeInTicks = (endTimestamp - _startTimestamp) / Stopwatch.Frequency * TimeSpan.TicksPerSecond;

            if ( idleTimeInTicks < MaxIdleTimeInTicks )
                return ZonedDateTime.CreateUtc( _utcStart.AddTicks( (long)idleTimeInTicks ) ).ToTimeZone( TimeZone );

            _startTimestamp = Stopwatch.GetTimestamp();
            _utcStart = DateTime.UtcNow;
            return ZonedDateTime.CreateUtc( _utcStart ).ToTimeZone( TimeZone );
        }
    }
}
