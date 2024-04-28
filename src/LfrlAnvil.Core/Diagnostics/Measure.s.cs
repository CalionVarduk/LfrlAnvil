using System;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

/// <summary>
/// Allows to measure time elapsed during delegate invocations.
/// </summary>
public static class Measure
{
    /// <summary>
    /// Invokes the provided <paramref name="action"/> and measures elapsed time.
    /// </summary>
    /// <param name="action">Delegate to invoke and measure.</param>
    /// <returns>Time elapsed during <paramref name="action"/> invocation.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan Call(Action action)
    {
        var stopwatch = StopwatchSlim.Create();
        action();
        return stopwatch.ElapsedTime;
    }

    /// <summary>
    /// Invokes the provided <paramref name="func"/> and measures elapsed time.
    /// </summary>
    /// <param name="func">Delegate to invoke and measure.</param>
    /// <returns>
    /// A <see cref="CallResult{T}"/> that contains <paramref name="func"/> invocation result and time elapsed during said invocation.
    /// </returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CallResult<T> Call<T>(Func<T> func)
    {
        var stopwatch = StopwatchSlim.Create();
        var result = func();
        return new CallResult<T>( result, stopwatch.ElapsedTime );
    }

    /// <summary>
    /// Represents a lightweight result of an operation along with the measurement of time elapsed during said operation.
    /// </summary>
    /// <param name="Result">Operation's result.</param>
    /// <param name="ElapsedTime">Time elapsed during an operation.</param>
    /// <typeparam name="T">Result's type.</typeparam>
    public readonly record struct CallResult<T>(T Result, TimeSpan ElapsedTime);
}
