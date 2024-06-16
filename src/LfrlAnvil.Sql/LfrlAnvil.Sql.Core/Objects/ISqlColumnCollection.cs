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

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents a collection of SQL table columns.
/// </summary>
public interface ISqlColumnCollection : IReadOnlyCollection<ISqlColumn>
{
    /// <summary>
    /// Table that this collection belongs to.
    /// </summary>
    ISqlTable Table { get; }

    /// <summary>
    /// Checks whether or not a column with the provided <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when column exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(string name);

    /// <summary>
    /// Returns a column with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the column to return.</param>
    /// <returns>Existing column.</returns>
    /// <exception cref="KeyNotFoundException">When column does not exist.</exception>
    [Pure]
    ISqlColumn Get(string name);

    /// <summary>
    /// Attempts to return a column with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the column to return.</param>
    /// <returns>Existing column or null when column does not exist.</returns>
    [Pure]
    ISqlColumn? TryGet(string name);
}
