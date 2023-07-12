﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlValuesNode : SqlNodeBase
{
    private readonly Array _expressions;

    internal SqlValuesNode(SqlExpressionNode[,] expressions)
        : base( SqlNodeType.Values )
    {
        Ensure.IsGreaterThan( expressions.Length, 0, nameof( expressions ) + '.' + nameof( expressions.Length ) );
        _expressions = expressions;
        RowCount = expressions.GetLength( 0 );
        ColumnCount = expressions.GetLength( 1 );
    }

    internal SqlValuesNode(SqlExpressionNode[] expressions)
        : base( SqlNodeType.Values )
    {
        Ensure.IsNotEmpty( expressions, nameof( expressions ) );
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
            Ensure.IsInRange( rowIndex, 0, RowCount - 1, nameof( rowIndex ) );
            return GetRow( rowIndex );
        }
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var valueIndent = indent + DefaultIndent;
        builder.Append( "VALUES" );

        for ( var row = 0; row < RowCount; ++row )
        {
            var expressions = GetRow( row );
            builder.Indent( indent ).Append( '(' );

            foreach ( var expression in expressions )
            {
                AppendChildTo( builder.Indent( valueIndent ), expression, valueIndent );
                builder.Append( ',' );
            }

            builder.Length -= 1;
            builder.Indent( indent ).Append( ')' ).Append( ',' );
        }

        builder.Length -= 1;
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
