using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

public sealed class PreciseUtcDateTimeProvider : DateTimeProviderBase
{
    private DateTime _utcStart = DateTime.UtcNow;
    private double _startTimestamp = Stopwatch.GetTimestamp();

    public PreciseUtcDateTimeProvider()
        : this( ChronoConstants.TicksPerSecond ) { }

    public PreciseUtcDateTimeProvider(long maxIdleTimeInTicks)
        : base( DateTimeKind.Utc )
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
            return _utcStart.AddTicks( (long)idleTimeInTicks );

        _startTimestamp = Stopwatch.GetTimestamp();
        _utcStart = DateTime.UtcNow;
        return _utcStart;
    }
}