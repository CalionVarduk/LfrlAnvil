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
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

/// <inheritdoc cref="ZonedClockBase" />
public sealed class ZonedClock : ZonedClockBase
{
    /// <summary>
    /// <see cref="ZonedClock"/> instance that returns <see cref="ZonedDateTime"/> instances
    /// in the <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    public static readonly ZonedClock Utc = new ZonedClock( TimeZoneInfo.Utc );

    /// <summary>
    /// <see cref="ZonedClock"/> instance that returns <see cref="ZonedDateTime"/> instances
    /// in the <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    public static readonly ZonedClock Local = new ZonedClock( TimeZoneInfo.Local );

    /// <summary>
    /// Creates a new <see cref="ZonedClock"/> instance.
    /// </summary>
    /// <param name="timeZone">Time zone of this clock.</param>
    public ZonedClock(TimeZoneInfo timeZone)
        : base( timeZone ) { }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override ZonedDateTime GetNow()
    {
        return ZonedDateTime.CreateUtc( DateTime.UtcNow ).ToTimeZone( TimeZone );
    }
}
