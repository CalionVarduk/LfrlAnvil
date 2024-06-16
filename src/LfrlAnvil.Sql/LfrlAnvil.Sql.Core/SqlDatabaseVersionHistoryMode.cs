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

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents how <see cref="ISqlDatabaseFactory"/> should handle version history table records.
/// </summary>
public enum SqlDatabaseVersionHistoryMode : byte
{
    /// <summary>
    /// Specifies that all version history table records should be included.
    /// </summary>
    AllRecords = 0,

    /// <summary>
    /// Specifies that only the last registered version history table record should be included.
    /// </summary>
    LastRecordOnly = 1
}
