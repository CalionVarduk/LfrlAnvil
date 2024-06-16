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
/// Contains <see cref="TimeZoneInfo.TransitionTime"/> extension methods.
/// </summary>
public static class TransitionTimeExtensions
{
    /// <summary>
    /// Creates a new <see cref="DateTime"/> instance from the provided <see cref="TimeZoneInfo.TransitionTime"/>
    /// for the given <paramref name="year"/>, that represents the start of the transition time.
    /// </summary>
    /// <param name="transitionTime">Source transition time.</param>
    /// <param name="year">Year for which to generate a <see cref="DateTime"/> instance.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    public static DateTime ToDateTime(this TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        return transitionTime.IsFixedDateRule
            ? FixedTimeToDateTime( transitionTime, year )
            : FloatingTimeToDateTime( transitionTime, year );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateTime FixedTimeToDateTime(TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        var daysInMonth = DateTime.DaysInMonth( year, transitionTime.Month );
        var day = Math.Min( transitionTime.Day, daysInMonth );
        var result = new DateTime( year, transitionTime.Month, day ) + transitionTime.TimeOfDay.TimeOfDay;
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateTime FloatingTimeToDateTime(TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        return transitionTime.Week <= 4
            ? FloatingTimeFromStartOfMonthToDateTime( transitionTime, year )
            : FloatingTimeFromEndOfMonthToDateTime( transitionTime, year );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateTime FloatingTimeFromStartOfMonthToDateTime(TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        var result = new DateTime( year, transitionTime.Month, 1 ) + transitionTime.TimeOfDay.TimeOfDay;

        var daysToAdd = ( int )transitionTime.DayOfWeek - ( int )result.DayOfWeek;
        if ( daysToAdd < 0 )
            daysToAdd += ChronoConstants.DaysPerWeek;

        daysToAdd += ChronoConstants.DaysPerWeek * (transitionTime.Week - 1);

        if ( daysToAdd > 0 )
            result = result.AddDays( daysToAdd );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DateTime FloatingTimeFromEndOfMonthToDateTime(TimeZoneInfo.TransitionTime transitionTime, int year)
    {
        var daysInMonth = DateTime.DaysInMonth( year, transitionTime.Month );
        var result = new DateTime( year, transitionTime.Month, daysInMonth ) + transitionTime.TimeOfDay.TimeOfDay;

        var dayDelta = ( int )result.DayOfWeek - ( int )transitionTime.DayOfWeek;
        if ( dayDelta < 0 )
            dayDelta += ChronoConstants.DaysPerWeek;

        if ( dayDelta > 0 )
            result = result.AddDays( -dayDelta );

        return result;
    }
}
