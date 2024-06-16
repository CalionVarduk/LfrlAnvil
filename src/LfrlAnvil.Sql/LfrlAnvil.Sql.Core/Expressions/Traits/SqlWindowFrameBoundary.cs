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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents a boundary of an <see cref="SqlWindowFrameNode"/>.
/// </summary>
public readonly struct SqlWindowFrameBoundary
{
    /// <summary>
    /// Represents a <see cref="SqlWindowFrameBoundaryDirection.CurrentRow"/> boundary.
    /// </summary>
    public static readonly SqlWindowFrameBoundary CurrentRow = new SqlWindowFrameBoundary(
        SqlWindowFrameBoundaryDirection.CurrentRow,
        null );

    /// <summary>
    /// Represents an unlimited <see cref="SqlWindowFrameBoundaryDirection.Preceding"/> boundary.
    /// </summary>
    public static readonly SqlWindowFrameBoundary UnboundedPreceding = new SqlWindowFrameBoundary(
        SqlWindowFrameBoundaryDirection.Preceding,
        null );

    /// <summary>
    /// Represents an unlimited <see cref="SqlWindowFrameBoundaryDirection.Following"/> boundary.
    /// </summary>
    public static readonly SqlWindowFrameBoundary UnboundedFollowing = new SqlWindowFrameBoundary(
        SqlWindowFrameBoundaryDirection.Following,
        null );

    private SqlWindowFrameBoundary(SqlWindowFrameBoundaryDirection direction, SqlExpressionNode? expression)
    {
        Direction = direction;
        Expression = expression;
    }

    /// <summary>
    /// <see cref="SqlWindowFrameBoundaryDirection"/> of this boundary.
    /// </summary>
    public SqlWindowFrameBoundaryDirection Direction { get; }

    /// <summary>
    /// Optional offset expression.
    /// </summary>
    public SqlExpressionNode? Expression { get; }

    /// <summary>
    /// Creates a new <see cref="SqlWindowFrameBoundary"/> instance with <see cref="SqlWindowFrameBoundaryDirection.Preceding"/> direction
    /// and a custom offset <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">Offset expression to use.</param>
    /// <returns>New <see cref="SqlWindowFrameBoundary"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlWindowFrameBoundary Preceding(SqlExpressionNode expression)
    {
        return new SqlWindowFrameBoundary( SqlWindowFrameBoundaryDirection.Preceding, expression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlWindowFrameBoundary"/> instance with <see cref="SqlWindowFrameBoundaryDirection.Following"/> direction
    /// and a custom offset <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">Offset expression to use.</param>
    /// <returns>New <see cref="SqlWindowFrameBoundary"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlWindowFrameBoundary Following(SqlExpressionNode expression)
    {
        return new SqlWindowFrameBoundary( SqlWindowFrameBoundaryDirection.Following, expression );
    }
}
