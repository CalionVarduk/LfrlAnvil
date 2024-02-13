using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Statements;

public readonly struct SqlQueryReaderRow
{
    internal SqlQueryReaderRow(SqlQueryReaderRowCollection source, int index)
    {
        Source = source;
        Index = index;
    }

    public SqlQueryReaderRowCollection Source { get; }
    public int Index { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public object? GetValue(int ordinal)
    {
        return Source.GetValue( Index, ordinal );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public object? GetValue(string fieldName)
    {
        return GetValue( Source.GetOrdinal( fieldName ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<object?> AsSpan()
    {
        return Source.GetRowSpan( Index );
    }

    [Pure]
    public object?[] ToArray()
    {
        var fields = Source.Fields;
        var result = new object?[fields.Length];
        for ( var i = 0; i < result.Length; ++i )
            result[i] = Source.GetValue( Index, fields[i].Ordinal );

        return result;
    }

    [Pure]
    public Dictionary<string, object?> ToDictionary()
    {
        var fields = Source.Fields;
        var result = new Dictionary<string, object?>( capacity: fields.Length, comparer: SqlHelpers.NameComparer );
        foreach ( var field in fields )
            result.Add( field.Name, Source.GetValue( Index, field.Ordinal ) );

        return result;
    }
}
