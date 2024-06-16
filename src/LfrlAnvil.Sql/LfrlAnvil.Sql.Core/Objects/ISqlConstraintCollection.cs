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
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents a collection of SQL table constraints.
/// </summary>
public interface ISqlConstraintCollection : IReadOnlyCollection<ISqlConstraint>
{
    /// <summary>
    /// Table that this collection belongs to.
    /// </summary>
    ISqlTable Table { get; }

    /// <summary>
    /// Table's primary key constraint.
    /// </summary>
    ISqlPrimaryKey PrimaryKey { get; }

    /// <summary>
    /// Checks whether or not a constraint with the provided <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when constraint exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(string name);

    /// <summary>
    /// Returns a constraint with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the constraint to return.</param>
    /// <returns>Existing constraint.</returns>
    /// <exception cref="KeyNotFoundException">When constraint does not exist.</exception>
    [Pure]
    ISqlConstraint Get(string name);

    /// <summary>
    /// Attempts to return a constraint with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the constraint to return.</param>
    /// <returns>Existing constraint or null when constraint does not exist.</returns>
    [Pure]
    ISqlConstraint? TryGet(string name);

    /// <summary>
    /// Returns an index with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the index to return.</param>
    /// <returns>Existing index.</returns>
    /// <exception cref="KeyNotFoundException">When index does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When constraint exists but is not an index.</exception>
    [Pure]
    ISqlIndex GetIndex(string name);

    /// <summary>
    /// Attempts to return an index with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the index to return.</param>
    /// <returns>Existing index or null when index does not exist.</returns>
    [Pure]
    ISqlIndex? TryGetIndex(string name);

    /// <summary>
    /// Returns a foreign key with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the foreign key to return.</param>
    /// <returns>Existing foreign key.</returns>
    /// <exception cref="KeyNotFoundException">When foreign key does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When constraint exists but is not a foreign key.</exception>
    [Pure]
    ISqlForeignKey GetForeignKey(string name);

    /// <summary>
    /// Attempts to return a foreign key with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the foreign key to return.</param>
    /// <returns>Existing foreign key or null when foreign key does not exist.</returns>
    [Pure]
    ISqlForeignKey? TryGetForeignKey(string name);

    /// <summary>
    /// Returns a check with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the check to return.</param>
    /// <returns>Existing check.</returns>
    /// <exception cref="KeyNotFoundException">When check does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When constraint exists but is not a check.</exception>
    [Pure]
    ISqlCheck GetCheck(string name);

    /// <summary>
    /// Attempts to return a check with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the check to return.</param>
    /// <returns>Existing check or null when check does not exist.</returns>
    [Pure]
    ISqlCheck? TryGetCheck(string name);
}
