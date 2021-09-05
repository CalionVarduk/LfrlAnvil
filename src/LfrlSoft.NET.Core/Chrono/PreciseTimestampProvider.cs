using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono
{
    public sealed class PreciseTimestampProvider : ITimestampProvider
    {
        private long _utcStartTicks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;
        private double _startTimestamp = Stopwatch.GetTimestamp();

        public PreciseTimestampProvider()
            : this( Constants.TicksPerSecond ) { }

        public PreciseTimestampProvider(long maxIdleTimeInTicks)
        {
            Ensure.IsGreaterThan( maxIdleTimeInTicks, 0, nameof( maxIdleTimeInTicks ) );
            MaxIdleTimeInTicks = maxIdleTimeInTicks;
        }

        public double MaxIdleTimeInTicks { get; }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Timestamp GetNow()
        {
            var endTimestamp = Stopwatch.GetTimestamp();
            var idleTimeInTicks = (endTimestamp - _startTimestamp) / Stopwatch.Frequency * TimeSpan.TicksPerSecond;

            if ( idleTimeInTicks < MaxIdleTimeInTicks )
                return new Timestamp( _utcStartTicks + (long) idleTimeInTicks );

            _startTimestamp = Stopwatch.GetTimestamp();
            _utcStartTicks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;
            return new Timestamp( _utcStartTicks );
        }
    }
}
