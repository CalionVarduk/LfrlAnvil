using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions
{
    public static class AdjustmentRuleExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static TimeZoneInfo.TransitionTime GetTransitionTimeWithInvalidity(this TimeZoneInfo.AdjustmentRule rule)
        {
            return rule.DaylightDelta >= TimeSpan.Zero ? rule.DaylightTransitionStart : rule.DaylightTransitionEnd;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static TimeZoneInfo.TransitionTime GetTransitionTimeWithAmbiguity(this TimeZoneInfo.AdjustmentRule rule)
        {
            return rule.DaylightDelta >= TimeSpan.Zero ? rule.DaylightTransitionEnd : rule.DaylightTransitionStart;
        }
    }
}
