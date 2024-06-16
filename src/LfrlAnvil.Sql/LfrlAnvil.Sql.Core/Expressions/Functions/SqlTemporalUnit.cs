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

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents available temporal units for date and/or time related functions.
/// </summary>
public enum SqlTemporalUnit : byte
{
    /// <summary>
    /// Specifies a nanosecond time unit.
    /// </summary>
    Nanosecond = 0,

    /// <summary>
    /// Specifies a microsecond time unit.
    /// </summary>
    Microsecond = 1,

    /// <summary>
    /// Specifies a millisecond time unit.
    /// </summary>
    Millisecond = 2,

    /// <summary>
    /// Specifies a second time unit.
    /// </summary>
    Second = 3,

    /// <summary>
    /// Specifies a minute time unit.
    /// </summary>
    Minute = 4,

    /// <summary>
    /// Specifies a hour time unit.
    /// </summary>
    Hour = 5,

    /// <summary>
    /// Specifies a day date unit.
    /// </summary>
    Day = 6,

    /// <summary>
    /// Specifies a week date unit.
    /// </summary>
    Week = 7,

    /// <summary>
    /// Specifies a month date unit.
    /// </summary>
    Month = 8,

    /// <summary>
    /// Specifies a year date unit.
    /// </summary>
    Year = 9
}
