﻿// Copyright 2024 Łukasz Furlepa
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

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="TimeSpan"/> extension methods.
/// </summary>
public static class TimeSpanExtensions
{
    /// <summary>
    /// Creates a new <see cref="TimeSpan"/> instance by calculating an absolute value from this instance.
    /// </summary>
    /// <param name="ts">Source timespan.</param>
    /// <returns>New <see cref="TimeSpan"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan Abs(this TimeSpan ts)
    {
        return TimeSpan.FromTicks( Math.Abs( ts.Ticks ) );
    }
}
