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
