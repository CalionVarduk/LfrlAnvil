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
/// Represents the mode with which to create <see cref="ISqlDatabase"/> instances.
/// </summary>
public enum SqlDatabaseCreateMode : byte
{
    /// <summary>
    /// Specifies that versions that haven't been applied to the database yet should not be invoked at all.
    /// </summary>
    NoChanges = 0,

    /// <summary>
    /// Specifies that versions that haven't been applied to the database yet should be ran in read-only mode.
    /// Such versions won't actually be applied to the database itself, but this mode can be useful for debugging
    /// created SQL statements that would be executed in <see cref="Commit"/> mode.
    /// </summary>
    DryRun = 1,

    /// <summary>
    /// Specifies that versions that haven't been applied to the database yet should be applied.
    /// </summary>
    Commit = 2
}
