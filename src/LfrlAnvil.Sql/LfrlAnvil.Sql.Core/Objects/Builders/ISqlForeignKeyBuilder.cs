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

using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL foreign key constraint builder.
/// </summary>
public interface ISqlForeignKeyBuilder : ISqlConstraintBuilder
{
    /// <summary>
    /// SQL index referenced by this foreign key.
    /// </summary>
    ISqlIndexBuilder ReferencedIndex { get; }

    /// <summary>
    /// SQL index that this foreign key originates from.
    /// </summary>
    ISqlIndexBuilder OriginIndex { get; }

    /// <summary>
    /// Specifies this foreign key's on delete behavior.
    /// </summary>
    ReferenceBehavior OnDeleteBehavior { get; }

    /// <summary>
    /// Specifies this foreign key's on update behavior.
    /// </summary>
    ReferenceBehavior OnUpdateBehavior { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    new ISqlForeignKeyBuilder SetName(string name);

    /// <inheritdoc cref="ISqlConstraintBuilder.SetDefaultName()" />
    new ISqlForeignKeyBuilder SetDefaultName();

    /// <summary>
    /// Changes <see cref="OnDeleteBehavior"/> value of this foreign key.
    /// </summary>
    /// <param name="behavior">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When behavior cannot be changed.</exception>
    ISqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior);

    /// <summary>
    /// Changes <see cref="OnUpdateBehavior"/> value of this foreign key.
    /// </summary>
    /// <param name="behavior">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When behavior cannot be changed.</exception>
    ISqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior);
}
