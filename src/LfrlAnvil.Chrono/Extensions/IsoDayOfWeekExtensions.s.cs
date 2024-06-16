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

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="IsoDayOfWeek"/> extension methods.
/// </summary>
public static class IsoDayOfWeekExtensions
{
    /// <summary>
    /// Converts the provided <paramref name="dayOfWeek"/> to <see cref="DayOfWeek"/> type.
    /// </summary>
    /// <param name="dayOfWeek">Value to convert.</param>
    /// <returns>New <see cref="DayOfWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DayOfWeek ToBcl(this IsoDayOfWeek dayOfWeek)
    {
        return ( DayOfWeek )(( int )dayOfWeek % 7);
    }
}
