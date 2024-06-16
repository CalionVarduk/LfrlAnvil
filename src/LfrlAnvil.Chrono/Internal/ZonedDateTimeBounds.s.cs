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
using LfrlAnvil.Chrono.Extensions;

namespace LfrlAnvil.Chrono.Internal;

internal static class ZonedDateTimeBounds
{
    [Pure]
    internal static BoundsRange<ZonedDateTime> CreateChecked(ZonedDateTime start, ZonedDateTime end)
    {
        var firstBounds = TryGetFirstBounds( start );
        var lastBounds = TryGetLastBounds( end );

        if ( firstBounds is null )
        {
            if ( lastBounds is null )
                return BoundsRange.Create( Bounds.Create( start, end ) );

            return BoundsRange.Create(
                new[] { Bounds.Create( start, lastBounds.Value.SecondToLastEnd ), Bounds.Create( lastBounds.Value.LastStart, end ) } );
        }

        if ( lastBounds is null )
        {
            return BoundsRange.Create(
                new[] { Bounds.Create( start, firstBounds.Value.FirstEnd ), Bounds.Create( firstBounds.Value.SecondStart, end ) } );
        }

        return BoundsRange.Create(
            new[]
            {
                Bounds.Create( start, firstBounds.Value.FirstEnd ),
                Bounds.Create( firstBounds.Value.SecondStart, lastBounds.Value.SecondToLastEnd ),
                Bounds.Create( lastBounds.Value.LastStart, end )
            } );
    }

    [Pure]
    internal static (ZonedDateTime SecondToLastEnd, ZonedDateTime LastStart)? TryGetLastBounds(ZonedDateTime end)
    {
        var oppositeEnd = end.GetOppositeAmbiguousDateTime();
        if ( oppositeEnd is null )
            return null;

        var activeRule = end.TimeZone.GetActiveAdjustmentRule( end.Value );
        Assume.IsNotNull( activeRule );
        var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

        var startDelta = activeRule.DaylightDelta.Abs() - transitionTime.TimeOfDay.TimeOfDay - TimeSpan.FromTicks( 1 );
        var lastStart = end.Subtract( new Duration( startDelta ) );

        return (oppositeEnd.Value, lastStart);
    }

    [Pure]
    internal static (ZonedDateTime FirstEnd, ZonedDateTime SecondStart)? TryGetFirstBounds(ZonedDateTime start)
    {
        var oppositeStart = start.GetOppositeAmbiguousDateTime();
        if ( oppositeStart is null )
            return null;

        var activeRule = start.TimeZone.GetActiveAdjustmentRule( start.Value );
        Assume.IsNotNull( activeRule );
        var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

        var endDelta = transitionTime.TimeOfDay.TimeOfDay - TimeSpan.FromTicks( 1 );
        var firstEnd = start.Add( new Duration( endDelta ) );

        return (firstEnd, oppositeStart.Value);
    }
}
