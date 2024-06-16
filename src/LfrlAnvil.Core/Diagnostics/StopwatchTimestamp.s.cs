// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

/// <summary>
/// Contains conversions between <see cref="Stopwatch"/> ticks and <see cref="TimeSpan"/> ticks.
/// </summary>
public static class StopwatchTimestamp
{
    /// <summary>
    /// Ratio of how many <see cref="TimeSpan"/> ticks there are in a single <see cref="Stopwatch"/> tick.
    /// </summary>
    public static readonly double TicksPerStopwatchTick = ( double )TimeSpan.TicksPerSecond / Stopwatch.Frequency;

    /// <summary>
    /// Converts time measured in <see cref="Stopwatch"/> ticks to <see cref="TimeSpan"/> ticks.
    /// </summary>
    /// <param name="start">Start of <see cref="Stopwatch"/> time measurement.</param>
    /// <param name="end">End of <see cref="Stopwatch"/> time measurement.</param>
    /// <returns>Time elapsed in <see cref="TimeSpan"/> ticks.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long GetTicks(long start, long end)
    {
        var elapsedTicks = unchecked( end - start ) * TicksPerStopwatchTick;
        return unchecked( ( long )elapsedTicks );
    }

    /// <summary>
    /// Converts time measured in <see cref="Stopwatch"/> ticks to <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="start">Start of <see cref="Stopwatch"/> time measurement.</param>
    /// <param name="end">End of <see cref="Stopwatch"/> time measurement.</param>
    /// <returns>Time elapsed in <see cref="TimeSpan"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan GetTimeSpan(long start, long end)
    {
        return TimeSpan.FromTicks( GetTicks( start, end ) );
    }

    /// <summary>
    /// Converts time measures in <see cref="TimeSpan"/> ticks to <see cref="Stopwatch"/> ticks.
    /// </summary>
    /// <param name="ticks">Time elapsed in <see cref="TimeSpan"/> ticks.</param>
    /// <returns>Time elapsed in <see cref="Stopwatch"/> ticks.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long GetStopwatchTicks(long ticks)
    {
        var stopwatchTicks = ticks / TicksPerStopwatchTick;
        return checked( ( long )stopwatchTicks );
    }

    /// <summary>
    /// Converts time measures in <see cref="TimeSpan"/> to <see cref="Stopwatch"/> ticks.
    /// </summary>
    /// <param name="timeSpan">Time elapsed.</param>
    /// <returns>Time elapsed in <see cref="Stopwatch"/> ticks.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long GetStopwatchTicks(TimeSpan timeSpan)
    {
        return GetStopwatchTicks( timeSpan.Ticks );
    }
}
