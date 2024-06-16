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

using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL table.
/// </summary>
public interface ISqlTable : ISqlObject
{
    /// <summary>
    /// Schema that this table belongs to.
    /// </summary>
    ISqlSchema Schema { get; }

    /// <summary>
    /// Collection of columns that belong to this table.
    /// </summary>
    ISqlColumnCollection Columns { get; }

    /// <summary>
    /// Collection of constraints that belong to this table.
    /// </summary>
    ISqlConstraintCollection Constraints { get; }

    /// <summary>
    /// Represents a full name information of this table.
    /// </summary>
    SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Underlying <see cref="SqlTableNode"/> instance that represents this table.
    /// </summary>
    SqlTableNode Node { get; }
}
