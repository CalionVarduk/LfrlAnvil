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
/// Contains <see cref="TimeZoneInfo.AdjustmentRule"/> extension methods.
/// </summary>
public static class AdjustmentRuleExtensions
{
    /// <summary>
    /// Returns <see cref="TimeZoneInfo.TransitionTime"/> instance that contains information
    /// about potentially invalid <see cref="DateTime"/> instances.
    /// </summary>
    /// <param name="rule">Source rule.</param>
    /// <returns>
    /// <see cref="TimeZoneInfo.TransitionTime"/> instance that contains information
    /// about potentially invalid <see cref="DateTime"/> instances.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeZoneInfo.TransitionTime GetTransitionTimeWithInvalidity(this TimeZoneInfo.AdjustmentRule rule)
    {
        return rule.DaylightDelta >= TimeSpan.Zero ? rule.DaylightTransitionStart : rule.DaylightTransitionEnd;
    }

    /// <summary>
    /// Returns <see cref="TimeZoneInfo.TransitionTime"/> instance that contains information
    /// about potentially ambiguous <see cref="DateTime"/> instances.
    /// </summary>
    /// <param name="rule">Source rule.</param>
    /// <returns>
    /// <see cref="TimeZoneInfo.TransitionTime"/> instance that contains information
    /// about potentially ambiguous <see cref="DateTime"/> instances.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeZoneInfo.TransitionTime GetTransitionTimeWithAmbiguity(this TimeZoneInfo.AdjustmentRule rule)
    {
        return rule.DaylightDelta >= TimeSpan.Zero ? rule.DaylightTransitionEnd : rule.DaylightTransitionStart;
    }
}
