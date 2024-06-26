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

namespace LfrlAnvil.Sqlite;

/// <summary>
/// Represents an SQLite DB encoding.
/// </summary>
public enum SqliteDatabaseEncoding : byte
{
    /// <summary>
    /// <b>UTF-8</b> encoding.
    /// </summary>
    UTF_8 = 0,

    /// <summary>
    /// <b>UTF-16</b> encoding.
    /// </summary>
    UTF_16 = 1,

    /// <summary>
    /// <b>UTF-16le</b> encoding.
    /// </summary>
    UTF_16_LE = 2,

    /// <summary>
    /// <b>UTF-16be</b> encoding.
    /// </summary>
    UTF_16_BE = 3
}
