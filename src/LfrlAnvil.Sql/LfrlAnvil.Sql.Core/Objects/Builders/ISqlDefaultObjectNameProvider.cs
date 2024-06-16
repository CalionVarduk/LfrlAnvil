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

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a provider of default SQL object names.
/// </summary>
public interface ISqlDefaultObjectNameProvider
{
    /// <summary>
    /// Creates a default primary key constraint name.
    /// </summary>
    /// <param name="table"><see cref="ISqlTableBuilder"/> that the primary key belongs to.</param>
    /// <returns>Default primary key constraint name.</returns>
    [Pure]
    string GetForPrimaryKey(ISqlTableBuilder table);

    /// <summary>
    /// Creates a default foreign key constraint name.
    /// </summary>
    /// <param name="originIndex"><see cref="ISqlIndexBuilder"/> from which the foreign key originates.</param>
    /// <param name="referencedIndex"><see cref="ISqlIndexBuilder"/> which the foreign key references.</param>
    /// <returns>Default foreign key constraint name.</returns>
    [Pure]
    string GetForForeignKey(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);

    /// <summary>
    /// Creates a default check constraint name.
    /// </summary>
    /// <param name="table"><see cref="ISqlTableBuilder"/> that the check belongs to.</param>
    /// <returns>Default check constraint name.</returns>
    [Pure]
    string GetForCheck(ISqlTableBuilder table);

    /// <summary>
    /// Creates a default index constraint name.
    /// </summary>
    /// <param name="table"><see cref="ISqlTableBuilder"/> that the index belongs to.</param>
    /// <param name="columns">Collection of columns that belong to the index.</param>
    /// <param name="isUnique">Specifies whether or not the index is unique.</param>
    /// <returns>Default index constraint name.</returns>
    [Pure]
    string GetForIndex(ISqlTableBuilder table, SqlIndexBuilderColumns<ISqlColumnBuilder> columns, bool isUnique);
}
