using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono
{
    public sealed class PreciseLocalDateTimeProvider : DateTimeProviderBase
    {
        private DateTime _start = DateTime.Now;
        private double _startTimestamp = Stopwatch.GetTimestamp();

        public PreciseLocalDateTimeProvider()
            : this( ChronoConstants.TicksPerSecond ) { }

        public PreciseLocalDateTimeProvider(long maxIdleTimeInTicks)
            : base( DateTimeKind.Local )
        {
            Ensure.IsGreaterThan( maxIdleTimeInTicks, 0, nameof( maxIdleTimeInTicks ) );
            MaxIdleTimeInTicks = maxIdleTimeInTicks;
        }

        public double MaxIdleTimeInTicks { get; }

        public override DateTime GetNow()
        {
            var endTimestamp = Stopwatch.GetTimestamp();
            var idleTimeInTicks = (endTimestamp - _startTimestamp) / Stopwatch.Frequency * TimeSpan.TicksPerSecond;

            if ( idleTimeInTicks < MaxIdleTimeInTicks )
                return _start.AddTicks( (long)idleTimeInTicks );

            _startTimestamp = Stopwatch.GetTimestamp();
            _start = DateTime.Now;
            return _start;
        }
    }
}
