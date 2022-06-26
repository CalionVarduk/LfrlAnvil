using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

public sealed class PreciseTimestampProvider : TimestampProviderBase
{
    private long _utcStartTicks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;
    private double _startTimestamp = Stopwatch.GetTimestamp();

    public PreciseTimestampProvider()
        : this( ChronoConstants.TicksPerSecond ) { }

    public PreciseTimestampProvider(long maxIdleTimeInTicks)
    {
        Ensure.IsGreaterThan( maxIdleTimeInTicks, 0, nameof( maxIdleTimeInTicks ) );
        MaxIdleTimeInTicks = maxIdleTimeInTicks;
    }

    public double MaxIdleTimeInTicks { get; }

    public override Timestamp GetNow()
    {
        var endTimestamp = Stopwatch.GetTimestamp();
        var idleTimeInTicks = (endTimestamp - _startTimestamp) / Stopwatch.Frequency * TimeSpan.TicksPerSecond;

        if ( idleTimeInTicks < MaxIdleTimeInTicks )
            return new Timestamp( _utcStartTicks + (long)idleTimeInTicks );

        _startTimestamp = Stopwatch.GetTimestamp();
        _utcStartTicks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;
        return new Timestamp( _utcStartTicks );
    }
}
