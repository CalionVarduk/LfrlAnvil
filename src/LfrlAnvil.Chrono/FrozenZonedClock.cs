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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a <see cref="ZonedDateTime"/> provider with a single frozen value.
/// </summary>
public sealed class FrozenZonedClock : ZonedClockBase
{
    private readonly ZonedDateTime _now;

    /// <summary>
    /// Creates a new <see cref="FrozenZonedClock"/> instance.
    /// </summary>
    /// <param name="now">Stored <see cref="ZonedDateTime"/> returned by this instance.</param>
    public FrozenZonedClock(ZonedDateTime now)
        : base( now.TimeZone )
    {
        _now = now;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override ZonedDateTime GetNow()
    {
        return _now;
    }
}
