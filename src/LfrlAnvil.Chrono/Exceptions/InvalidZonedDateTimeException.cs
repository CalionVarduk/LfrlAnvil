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
using LfrlAnvil.Chrono.Extensions;

namespace LfrlAnvil.Chrono.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid <see cref="DateTime"/> within a chosen <see cref="TimeZoneInfo"/>.
/// </summary>
public class InvalidZonedDateTimeException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidZonedDateTimeException"/> instance.
    /// </summary>
    /// <param name="dateTime">Invalid date time.</param>
    /// <param name="timeZone">Target time zone.</param>
    public InvalidZonedDateTimeException(DateTime dateTime, TimeZoneInfo timeZone)
        : base( CreateMessage( dateTime, timeZone ) )
    {
        DateTime = dateTime;
        TimeZone = timeZone;
    }

    /// <summary>
    /// Invalid date time.
    /// </summary>
    public DateTime DateTime { get; }

    /// <summary>
    /// Target time zone.
    /// </summary>
    public TimeZoneInfo TimeZone { get; }

    private static string CreateMessage(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var invalidityRange = timeZone.GetContainingInvalidityRange( dateTime );
        return invalidityRange is null
            ? Resources.InvalidDateTimeInTimeZone( dateTime, timeZone )
            : Resources.InvalidDateTimeInTimeZoneBecauseOfRange( dateTime, timeZone, invalidityRange.Value );
    }
}
