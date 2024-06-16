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
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Persistence;

namespace LfrlAnvil.Sqlite;

/// <summary>
/// Specifies available options for interpreting <see cref="SqlUpsertNode"/> instances.
/// </summary>
[Flags]
public enum SqliteUpsertOptions : byte
{
    /// <summary>
    /// Specifies that the upsert statement is not supported.
    /// </summary>
    /// <remarks>
    /// This setting will cause an interpreter to throw an <see cref="UnrecognizedSqlNodeException"/>
    /// whenever an <see cref="SqlUpsertNode"/> is visited.
    /// </remarks>
    Disabled = 0,

    /// <summary>
    /// Specifies that the upsert statement is supported.
    /// </summary>
    /// <remarks>
    /// Unless the <see cref="AllowEmptyConflictTarget"/> setting is also included,
    /// this setting requires that the <see cref="SqlUpsertNode.ConflictTarget"/> is either explicitly specified or
    /// is possible to be extracted from the target table.
    /// </remarks>
    Supported = 1,

    /// <summary>
    /// Specifies that the upsert statement is supported and that the <see cref="SqlUpsertNode.ConflictTarget"/> can be empty.
    /// </summary>
    AllowEmptyConflictTarget = 2
}
