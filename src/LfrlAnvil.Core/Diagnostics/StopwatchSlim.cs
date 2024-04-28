using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

/// <summary>
/// A lightweight version of the <see cref="Stopwatch"/> class.
/// </summary>
public readonly struct StopwatchSlim
{
    /// <summary>
    /// Creates a new <see cref="StopwatchSlim"/> instance.
    /// </summary>
    /// <param name="start">Start of time measurement in stopwatch ticks.</param>
    public StopwatchSlim(long start)
    {
        Start = start;
    }

    /// <summary>
    /// Start of time measurement in stopwatch ticks.
    /// </summary>
    public long Start { get; }

    /// <summary>
    /// Time elapsed between the <see cref="Start"/> of measurement and now.
    /// </summary>
    public TimeSpan ElapsedTime => StopwatchTimestamp.GetTimeSpan( Start, Stopwatch.GetTimestamp() );

    /// <summary>
    /// Creates a new <see cref="StopwatchSlim"/> instance with <see cref="Start"/>
    /// equal to <see cref="Stopwatch.GetTimestamp()"/> invocation result.
    /// </summary>
    /// <returns>New <see cref="StopwatchSlim"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StopwatchSlim Create()
    {
        return new StopwatchSlim( Stopwatch.GetTimestamp() );
    }
}
