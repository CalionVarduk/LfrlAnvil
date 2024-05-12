using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a single type-erased row.
/// </summary>
public readonly struct SqlQueryReaderRow
{
    internal SqlQueryReaderRow(SqlQueryReaderRowCollection source, int index)
    {
        Source = source;
        Index = index;
    }

    /// <summary>
    /// Source collection of rows.
    /// </summary>
    public SqlQueryReaderRowCollection Source { get; }

    /// <summary>
    /// Specifies the 0-based position of this row in the whole collection.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Returns a value of a field at the specified 0-based position.
    /// </summary>
    /// <param name="ordinal">Field's position.</param>
    /// <returns>Field's value.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// When <see cref="Index"/> and <paramref name="ordinal"/> combination is out of bounds.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public object? GetValue(int ordinal)
    {
        return Source.GetValue( Index, ordinal );
    }

    /// <summary>
    /// Returns a value of a field with the provided name.
    /// </summary>
    /// <param name="fieldName">Field's name.</param>
    /// <returns>Field's value.</returns>
    /// <exception cref="KeyNotFoundException">When field does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public object? GetValue(string fieldName)
    {
        return GetValue( Source.GetOrdinal( fieldName ) );
    }

    /// <summary>
    /// Converts this row to a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <returns><see cref="ReadOnlySpan{T}"/> equivalent to this row's values.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<object?> AsSpan()
    {
        return Source.GetRowSpan( Index );
    }

    /// <summary>
    /// Creates a new array of values from this row.
    /// </summary>
    /// <returns>New array equivalent to this row's values.</returns>
    [Pure]
    public object?[] ToArray()
    {
        var fields = Source.Fields;
        var result = new object?[fields.Length];
        for ( var i = 0; i < result.Length; ++i )
            result[i] = Source.GetValue( Index, fields[i].Ordinal );

        return result;
    }

    /// <summary>
    /// Creates a new collection of (field-name, value) pairs from this row.
    /// </summary>
    /// <returns>New collection of (field-name, value) pairs equivalent to this row's values.</returns>
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
