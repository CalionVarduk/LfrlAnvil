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

using System.Collections.Generic;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL index constraint builder.
/// </summary>
public interface ISqlIndexBuilder : ISqlConstraintBuilder
{
    /// <summary>
    /// Collection of columns that define this index.
    /// </summary>
    SqlIndexBuilderColumns<ISqlColumnBuilder> Columns { get; }

    /// <summary>
    /// Collection of columns referenced by this index's <see cref="Columns"/>.
    /// </summary>
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedColumns { get; }

    /// <summary>
    /// Collection of columns referenced by this index's <see cref="Filter"/>.
    /// </summary>
    IReadOnlyCollection<ISqlColumnBuilder> ReferencedFilterColumns { get; }

    /// <summary>
    /// Optional <see cref="ISqlPrimaryKeyBuilder"/> instance attached to this index.
    /// </summary>
    ISqlPrimaryKeyBuilder? PrimaryKey { get; }

    /// <summary>
    /// Specifies whether or not this index is unique.
    /// </summary>
    bool IsUnique { get; }

    /// <summary>
    /// Specifies whether or not this index is virtual.
    /// </summary>
    /// <remarks>Virtual indexes aren't actually created in the database.</remarks>
    bool IsVirtual { get; }

    /// <summary>
    /// Specifies an optional filter condition of this index.
    /// </summary>
    SqlConditionNode? Filter { get; }

    /// <summary>
    /// Changes <see cref="IsUnique"/> value of this index.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When uniqueness cannot be changed.</exception>
    ISqlIndexBuilder MarkAsUnique(bool enabled = true);

    /// <summary>
    /// Changes <see cref="IsVirtual"/> value of this index.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When virtuality cannot be changed.</exception>
    ISqlIndexBuilder MarkAsVirtual(bool enabled = true);

    /// <summary>
    /// Changes <see cref="Filter"/> value of this index.
    /// </summary>
    /// <param name="filter">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When filter cannot be changed.</exception>
    ISqlIndexBuilder SetFilter(SqlConditionNode? filter);

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlIndexBuilder SetName(string name);

    /// <inheritdoc cref="ISqlConstraintBuilder.SetDefaultName()" />
    new ISqlIndexBuilder SetDefaultName();
}
