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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="BoundsRange{T}"/> extension methods.
/// </summary>
public static class BoundsRangeExtensions
{
    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance by merging neighbouring bounds together when <see cref="Bounds{T}.Max"/>
    /// of the first bounds and <see cref="Bounds{T}.Min"/> of the second bounds differ by <b>1 tick</b>.
    /// </summary>
    /// <param name="source">Source bounds range.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    /// <remarks>See <see cref="BoundsRange{T}.Normalize(Func{T,T,Boolean})"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BoundsRange<DateTime> Normalize(this BoundsRange<DateTime> source)
    {
        return source.Normalize( static (a, b) => a + TimeSpan.FromTicks( 1 ) == b );
    }

    /// <summary>
    /// Calculates the total <see cref="TimeSpan"/> of the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source bounds range.</param>
    /// <returns>New <see cref="TimeSpan"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan GetTimeSpan(this BoundsRange<DateTime> source)
    {
        return source.Aggregate( TimeSpan.Zero, static (a, b) => a + b.GetTimeSpan() );
    }

    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance by merging neighbouring bounds together when
    /// <see cref="ZonedDateTime.Timestamp"/> of <see cref="Bounds{T}.Max"/> of the first bounds and
    /// <see cref="ZonedDateTime.Timestamp"/> of <see cref="Bounds{T}.Min"/> of the second bounds differ by <b>1 tick</b>.
    /// </summary>
    /// <param name="source">Source bounds range.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    /// <remarks>See <see cref="BoundsRange{T}.Normalize(Func{T,T,Boolean})"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BoundsRange<ZonedDateTime> Normalize(this BoundsRange<ZonedDateTime> source)
    {
        return source.Normalize( static (a, b) => a.Timestamp.Add( Duration.FromTicks( 1 ) ) == b.Timestamp );
    }

    /// <summary>
    /// Calculates the total <see cref="Duration"/> of the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source bounds range.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration GetDuration(this BoundsRange<ZonedDateTime> source)
    {
        return source.Aggregate( Duration.Zero, static (a, b) => a + b.GetDuration() );
    }

    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance by merging neighbouring bounds together when
    /// <see cref="ZonedDateTime.Timestamp"/> of <see cref="ZonedDay.End"/> of <see cref="Bounds{T}.Max"/> of the first bounds and
    /// <see cref="ZonedDateTime.Timestamp"/> of <see cref="ZonedDay.Start"/> of <see cref="Bounds{T}.Min"/> of the second bounds
    /// differ by <b>1 tick</b>.
    /// </summary>
    /// <param name="source">Source bounds range.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    /// <remarks>See <see cref="BoundsRange{T}.Normalize(Func{T,T,Boolean})"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BoundsRange<ZonedDay> Normalize(this BoundsRange<ZonedDay> source)
    {
        return source.Normalize( static (a, b) => a.End.Timestamp.Add( Duration.FromTicks( 1 ) ) == b.Start.Timestamp );
    }
}
