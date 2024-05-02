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
