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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an SQL table column.
/// </summary>
public interface ISqlColumn : ISqlObject
{
    /// <summary>
    /// Table that this column belongs to.
    /// </summary>
    ISqlTable Table { get; }

    /// <summary>
    /// <see cref="ISqlColumnTypeDefinition"/> instance that defines the data type of this column.
    /// </summary>
    ISqlColumnTypeDefinition TypeDefinition { get; }

    /// <summary>
    /// Specifies whether or not this column accepts null values.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    /// Specifies whether or not this column has a default value.
    /// </summary>
    bool HasDefaultValue { get; }

    /// <summary>
    /// Specifies the type of storage for this column's computation or null when this column is not computed.
    /// </summary>
    SqlColumnComputationStorage? ComputationStorage { get; }

    /// <summary>
    /// Underlying <see cref="SqlColumnNode"/> instance that represents this column.
    /// </summary>
    SqlColumnNode Node { get; }

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance from this column with <see cref="OrderBy.Asc"/> ordering.
    /// </summary>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    SqlOrderByNode Asc();

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance from this column with <see cref="OrderBy.Desc"/> ordering.
    /// </summary>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    SqlOrderByNode Desc();
}
