using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Sql.Expressions;

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

    public int RowCount { get; }
    public int ColumnCount { get; }

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
