using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

public static class Measure
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan Call(Action action)
    {
        var start = Stopwatch.GetTimestamp();
        action();
        var end = Stopwatch.GetTimestamp();
        return StopwatchTimestamp.GetTimeSpan( start, end );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CallResult<T> Call<T>(Func<T> func)
    {
        var start = Stopwatch.GetTimestamp();
        var result = func();
        var end = Stopwatch.GetTimestamp();
        return new CallResult<T>( result, StopwatchTimestamp.GetTimeSpan( start, end ) );
    }

    public readonly record struct CallResult<T>(T Result, TimeSpan ElapsedTime);
}
