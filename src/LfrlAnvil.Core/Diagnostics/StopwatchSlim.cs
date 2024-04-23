using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

public readonly struct StopwatchSlim
{
    public StopwatchSlim(long start)
    {
        Start = start;
    }

    public long Start { get; }
    public TimeSpan ElapsedTime => StopwatchTimestamp.GetTimeSpan( Start, Stopwatch.GetTimestamp() );

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StopwatchSlim Create()
    {
        return new StopwatchSlim( Stopwatch.GetTimestamp() );
    }
}
