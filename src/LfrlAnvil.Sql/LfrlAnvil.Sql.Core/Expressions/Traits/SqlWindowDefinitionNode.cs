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

namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a window.
/// </summary>
public sealed class SqlWindowDefinitionNode : SqlNodeBase
{
    internal SqlWindowDefinitionNode(
        string name,
        SqlExpressionNode[] partitioning,
        SqlOrderByNode[] ordering,
        SqlWindowFrameNode? frame)
        : base( SqlNodeType.WindowDefinition )
    {
        Name = name;
        Partitioning = partitioning;
        Ordering = ordering;
        Frame = frame;
    }

    /// <summary>
    /// Window's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Collection of expressions by which this window partitions the result set.
    /// </summary>
    public ReadOnlyArray<SqlExpressionNode> Partitioning { get; }

    /// <summary>
    /// Collection of ordering expressions used by this window.
    /// </summary>
    public ReadOnlyArray<SqlOrderByNode> Ordering { get; }

    /// <summary>
    /// Optional <see cref="SqlWindowFrameNode"/> instance that defines the frame of this window.
    /// </summary>
    public SqlWindowFrameNode? Frame { get; }
}
