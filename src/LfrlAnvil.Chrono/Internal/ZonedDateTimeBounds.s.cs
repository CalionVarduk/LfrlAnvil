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
                new[]
                {
                    Bounds.Create( start, lastBounds.Value.SecondToLastEnd ),
                    Bounds.Create( lastBounds.Value.LastStart, end )
                } );
        }

        if ( lastBounds is null )
        {
            return BoundsRange.Create(
                new[]
                {
                    Bounds.Create( start, firstBounds.Value.FirstEnd ),
                    Bounds.Create( firstBounds.Value.SecondStart, end )
                } );
        }

        return BoundsRange.Create(
            new[]
            {
                Bounds.Create( start, firstBounds.Value.FirstEnd ),
                Bounds.Create( firstBounds.Value.SecondStart, lastBounds.Value.SecondToLastEnd ),
                Bounds.Create( lastBounds.Value.LastStart, end )
            } );
    }

    internal static (ZonedDateTime SecondToLastEnd, ZonedDateTime LastStart)? TryGetLastBounds(ZonedDateTime end)
    {
        var oppositeEnd = end.GetOppositeAmbiguousDateTime();
        if ( oppositeEnd is null )
            return null;

        var activeRule = end.TimeZone.GetActiveAdjustmentRule( end.Value )!;
        var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

        var startDelta = activeRule.DaylightDelta.Abs() - transitionTime.TimeOfDay.TimeOfDay - TimeSpan.FromTicks( 1 );
        var lastStart = end.Subtract( new Duration( startDelta ) );

        return (oppositeEnd.Value, lastStart);
    }

    internal static (ZonedDateTime FirstEnd, ZonedDateTime SecondStart)? TryGetFirstBounds(ZonedDateTime start)
    {
        var oppositeStart = start.GetOppositeAmbiguousDateTime();
        if ( oppositeStart is null )
            return null;

        var activeRule = start.TimeZone.GetActiveAdjustmentRule( start.Value )!;
        var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

        var endDelta = transitionTime.TimeOfDay.TimeOfDay - TimeSpan.FromTicks( 1 );
        var firstEnd = start.Add( new Duration( endDelta ) );

        return (firstEnd, oppositeStart.Value);
    }
}