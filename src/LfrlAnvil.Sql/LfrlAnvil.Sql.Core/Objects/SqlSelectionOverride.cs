// Copyright 2026 Łukasz Furlepa
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
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents a default SQL selection node override.
/// </summary>
public readonly record struct SqlSelectionOverride
{
    private SqlSelectionOverride(SqlSelectNode? node, bool isIgnored)
    {
        Node = node;
        IsIgnored = isIgnored;
    }

    /// <summary>
    /// Node to use instead of a default selection.
    /// </summary>
    public readonly SqlSelectNode? Node;

    /// <summary>
    /// Specifies whether the selection should be completely ignored.
    /// </summary>
    public readonly bool IsIgnored;

    /// <summary>
    /// Represents usage of a default selection.
    /// </summary>
    public static SqlSelectionOverride UseDefault => new SqlSelectionOverride( null, false );

    /// <summary>
    /// Represents an ignored selection.
    /// </summary>
    public static SqlSelectionOverride Ignore => new SqlSelectionOverride( null, true );

    /// <summary>
    /// Creates an override using a custom <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Node to use instead of a default selection.</param>
    /// <returns>New <see cref="SqlSelectionOverride"/> instance.</returns>
    [Pure]
    public static implicit operator SqlSelectionOverride(SqlSelectNode node)
    {
        return new SqlSelectionOverride( node, false );
    }
}
