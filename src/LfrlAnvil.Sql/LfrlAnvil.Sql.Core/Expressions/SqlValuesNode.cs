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

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node that defines a 1-dimensional or 2-dimensional collection of values to insert.
/// </summary>
public sealed class SqlValuesNode : SqlNodeBase
{
    private readonly Array _expressions;

    internal SqlValuesNode(SqlExpressionNode[,] expressions)
        : base( SqlNodeType.Values )
    {
        Ensure.IsGreaterThan( expressions.Length, 0 );
        _expressions = expressions;
        RowCount = expressions.GetLength( 0 );
        ColumnCount = expressions.GetLength( 1 );
    }

    internal SqlValuesNode(SqlExpressionNode[] expressions)
        : base( SqlNodeType.Values )
    {
        Ensure.IsNotEmpty( expressions );
        _expressions = expressions;
        RowCount = 1;
        ColumnCount = expressions.Length;
    }

    /// <summary>
    /// Number of rows.
    /// </summary>
    public int RowCount { get; }

    /// <summary>
    /// Number of columns.
    /// </summary>
    public int ColumnCount { get; }

    /// <summary>
    /// Gets a collection of values associated with a row at the specified 0-based <paramref name="rowIndex"/>.
    /// </summary>
    /// <param name="rowIndex">Index of the row to get.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="rowIndex"/> is out of bounds.</exception>
    public ReadOnlySpan<SqlExpressionNode> this[int rowIndex]
    {
        get
        {
            Ensure.IsInIndexRange( rowIndex, RowCount );
            return GetRow( rowIndex );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ReadOnlySpan<SqlExpressionNode> GetRow(int index)
    {
        return MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.Add(
                ref Unsafe.As<byte, SqlExpressionNode>( ref MemoryMarshal.GetArrayDataReference( _expressions ) ),
                index * ColumnCount ),
            ColumnCount );
    }
}
