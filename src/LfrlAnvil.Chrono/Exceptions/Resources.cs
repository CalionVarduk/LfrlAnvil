using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono.Exceptions;

internal static class Resources
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidDateTimeInTimeZone(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var dateTimeText = TextFormatting.StringifyDateTime( dateTime );
        return $"{dateTimeText} is not a valid datetime in {timeZone.Id} timezone.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidDateTimeInTimeZoneBecauseOfRange(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        Bounds<DateTime> invalidityRange)
    {
        var dateTimeText = TextFormatting.StringifyDateTime( dateTime );
        var invalidityMinText = TextFormatting.StringifyDateTime( invalidityRange.Min );
        var invalidityMaxText = TextFormatting.StringifyDateTime( invalidityRange.Max );
        var invalidityText = $"{invalidityMinText}, {invalidityMaxText}";
        return $"{dateTimeText} is not a valid datetime in {timeZone.Id} timezone because it falls into the [{invalidityText}] range.";
    }
}