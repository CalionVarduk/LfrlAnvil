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

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents an ISO 8601 day of week.
/// </summary>
public enum IsoDayOfWeek : byte
{
    /// <summary>
    /// Unknown day of week.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Represents monday.
    /// </summary>
    Monday = 1,

    /// <summary>
    /// Represents tuesday.
    /// </summary>
    Tuesday = 2,

    /// <summary>
    /// Represents wednesday.
    /// </summary>
    Wednesday = 3,

    /// <summary>
    /// Represents thursday.
    /// </summary>
    Thursday = 4,

    /// <summary>
    /// Represents friday.
    /// </summary>
    Friday = 5,

    /// <summary>
    /// Represents saturday.
    /// </summary>
    Saturday = 6,

    /// <summary>
    /// Represents sunday.
    /// </summary>
    Sunday = 7
}
