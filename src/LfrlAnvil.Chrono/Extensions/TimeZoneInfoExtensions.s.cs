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

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="TimeZoneInfo"/> extension methods.
/// </summary>
public static class TimeZoneInfoExtensions
{
    /// <summary>
    /// Gets <see cref="DateTimeKind"/> associated with the specified <paramref name="timeZone"/>.
    /// </summary>
    /// <param name="timeZone">Source time zone.</param>
    /// <returns><see cref="DateTimeKind"/> associated with the specified <paramref name="timeZone"/>.</returns>
    [Pure]
    public static DateTimeKind GetDateTimeKind(this TimeZoneInfo timeZone)
    {
        if ( ReferenceEquals( timeZone, TimeZoneInfo.Utc ) )
            return DateTimeKind.Utc;

        if ( ReferenceEquals( timeZone, TimeZoneInfo.Local ) )
            return DateTimeKind.Local;

        return DateTimeKind.Unspecified;
    }

    /// <summary>
    /// Attempts to find the first <see cref="TimeZoneInfo.AdjustmentRule"/> instance that applies to the given <paramref name="dateTime"/>.
    /// </summary>
    /// <param name="timeZone">Source time zone.</param>
    /// <param name="dateTime">Target date time.</param>
    /// <returns>
    /// First <see cref="TimeZoneInfo.AdjustmentRule"/> instance that applies to the given <paramref name="dateTime"/>
    /// or null when none exists.
    /// </returns>
    [Pure]
    public static TimeZoneInfo.AdjustmentRule? GetActiveAdjustmentRule(this TimeZoneInfo timeZone, DateTime dateTime)
    {
        var date = dateTime.Date;
        var rules = timeZone.GetAdjustmentRules();

        for ( var i = 0; i < rules.Length; ++i )
        {
            var rule = rules[i];
            if ( rule.DateStart <= date && rule.DateEnd >= date )
                return rule;
        }

        return null;
    }

    /// <summary>
    /// Attempts to find the 0-based index of the first <see cref="TimeZoneInfo.AdjustmentRule"/> instance
    /// that applies to the given <paramref name="dateTime"/>.
    /// </summary>
    /// <param name="timeZone">Source time zone.</param>
    /// <param name="dateTime">Target date time.</param>
    /// <returns>
    /// 0-based index of the first <see cref="TimeZoneInfo.AdjustmentRule"/> instance that applies to the given <paramref name="dateTime"/>
    /// or <b>-1</b> when none exists.
    /// </returns>
    [Pure]
    public static int GetActiveAdjustmentRuleIndex(this TimeZoneInfo timeZone, DateTime dateTime)
    {
        var date = dateTime.Date;
        var rules = timeZone.GetAdjustmentRules();

        for ( var i = 0; i < rules.Length; ++i )
        {
            var rule = rules[i];
            if ( rule.DateStart <= date && rule.DateEnd >= date )
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Attempts to find a range of invalid <see cref="DateTime"/> instances defined by the provided <paramref name="timeZone"/>
    /// that contains the given <paramref name="dateTime"/>.
    /// </summary>
    /// <param name="timeZone">Source time zone.</param>
    /// <param name="dateTime">Target date time.</param>
    /// <returns>
    /// Range of invalid <see cref="DateTime"/> instances defined by the provided <paramref name="timeZone"/>
    /// that contains the given <paramref name="dateTime"/> or null, when <paramref name="dateTime"/> is valid.
    /// </returns>
    [Pure]
    public static Bounds<DateTime>? GetContainingInvalidityRange(this TimeZoneInfo timeZone, DateTime dateTime)
    {
        // NOTE: this can't really be safely optimized away, since .NET doesn't provide any public properties to handle DST disabling changes
        // https://techcommunity.microsoft.com/t5/daylight-saving-time-time-zone/how-does-microsoft-handle-dst-servicing-when-a-country-decides/ba-p/1175108
        if ( ! timeZone.IsInvalidTime( dateTime ) )
            return null;

        var activeRule = timeZone.GetActiveAdjustmentRule( dateTime );
        Assume.IsNotNull( activeRule );
        var transitionDateTime = GetInvalidityDateTimeRange( activeRule, dateTime.Year );

        if ( dateTime >= transitionDateTime.Start && dateTime < transitionDateTime.End )
            return Bounds.Create( transitionDateTime.Start, transitionDateTime.End.AddTicks( -1 ) );

        // NOTE: this doesn't seem to be correct, since activeRule may actually no longer be active in the previous year (or the rule may be floating)
        // however, this implementation corresponds to .NET's private GetIsInvalidTime method's body:
        // https://github.com/microsoft/referencesource/blob/5697c29004a34d80acdaf5742d7e699022c64ecd/mscorlib/system/timezoneinfo.cs#L1745
        // the main goal of GetContainingInvalidityRange extension is to correspond 1-to-1 to TimeZoneInfo.IsInvalidTime method's result
        // so for now, it must stay like this
        transitionDateTime = (transitionDateTime.Start.AddYears( -1 ), transitionDateTime.End.AddYears( -1 ));
        return Bounds.Create( transitionDateTime.Start, transitionDateTime.End.AddTicks( -1 ) );
    }

    /// <summary>
    /// Attempts to find a range of ambiguous <see cref="DateTime"/> instances defined by the provided <paramref name="timeZone"/>
    /// that contains the given <paramref name="dateTime"/>.
    /// </summary>
    /// <param name="timeZone">Source time zone.</param>
    /// <param name="dateTime">Target date time.</param>
    /// <returns>
    /// Range of ambiguous <see cref="DateTime"/> instances defined by the provided <paramref name="timeZone"/>
    /// that contains the given <paramref name="dateTime"/> or null, when <paramref name="dateTime"/> is not ambiguous.
    /// </returns>
    [Pure]
    public static Bounds<DateTime>? GetContainingAmbiguityRange(this TimeZoneInfo timeZone, DateTime dateTime)
    {
        // NOTE: this can't really be safely optimized away, since .NET doesn't provide any public properties to handle DST disabling changes
        // https://techcommunity.microsoft.com/t5/daylight-saving-time-time-zone/how-does-microsoft-handle-dst-servicing-when-a-country-decides/ba-p/1175108
        if ( ! timeZone.IsAmbiguousTime( dateTime ) )
            return null;

        var activeRule = timeZone.GetActiveAdjustmentRule( dateTime );
        Assume.IsNotNull( activeRule );
        var transitionDateTime = GetAmbiguityDateTimeRange( activeRule, dateTime.Year );

        if ( dateTime >= transitionDateTime.Start && dateTime < transitionDateTime.End )
            return Bounds.Create( transitionDateTime.Start, transitionDateTime.End.AddTicks( -1 ) );

        // NOTE: this doesn't seem to be correct, since activeRule may actually no longer be active in the next year (or the rule may be floating)
        // however, this implementation corresponds to .NET's private GetIsAmbiguousTime method's body:
        // https://github.com/microsoft/referencesource/blob/5697c29004a34d80acdaf5742d7e699022c64ecd/mscorlib/system/timezoneinfo.cs#L1681
        // the main goal of GetContainingAmbiguityRange extension is to correspond 1-to-1 to TimeZoneInfo.IsAmbiguousTime method's result
        // so for now, it must stay like this
        transitionDateTime = (transitionDateTime.Start.AddYears( 1 ), transitionDateTime.End.AddYears( 1 ));
        return Bounds.Create( transitionDateTime.Start, transitionDateTime.End.AddTicks( -1 ) );
    }

    private static (DateTime Start, DateTime End) GetInvalidityDateTimeRange(TimeZoneInfo.AdjustmentRule rule, int year)
    {
        if ( rule.DaylightDelta > TimeSpan.Zero )
        {
            var start = rule.DaylightTransitionStart.ToDateTime( year );
            return (start, start + rule.DaylightDelta);
        }
        else
        {
            var start = rule.DaylightTransitionEnd.ToDateTime( year );
            return (start, start - rule.DaylightDelta);
        }
    }

    private static (DateTime Start, DateTime End) GetAmbiguityDateTimeRange(TimeZoneInfo.AdjustmentRule rule, int year)
    {
        if ( rule.DaylightDelta > TimeSpan.Zero )
        {
            var end = rule.DaylightTransitionEnd.ToDateTime( year );
            return (end - rule.DaylightDelta, end);
        }
        else
        {
            var end = rule.DaylightTransitionStart.ToDateTime( year );
            return (end + rule.DaylightDelta, end);
        }
    }
}
