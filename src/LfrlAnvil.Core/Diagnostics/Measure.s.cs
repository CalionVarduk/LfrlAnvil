using System;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

public static class Measure
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan Call(Action action)
    {
        var stopwatch = StopwatchSlim.Create();
        action();
        return stopwatch.ElapsedTime;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CallResult<T> Call<T>(Func<T> func)
    {
        var stopwatch = StopwatchSlim.Create();
        var result = func();
        return new CallResult<T>( result, stopwatch.ElapsedTime );
    }

    public readonly record struct CallResult<T>(T Result, TimeSpan ElapsedTime);
}
