using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a collection of type-erased rows.
/// </summary>
public sealed class SqlQueryReaderRowCollection : IReadOnlyList<SqlQueryReaderRow>
{
    private readonly List<object?> _cells;
    private readonly SqlResultSetField[] _fields;
    private Dictionary<string, int>? _fieldOrdinals;

    internal SqlQueryReaderRowCollection(SqlResultSetField[] fields, List<object?> cells)
    {
        Ensure.IsGreaterThan( fields.Length, 0 );
        Ensure.Equals( cells.Count % fields.Length, 0 );

        Count = cells.Count / fields.Length;
        Assume.IsGreaterThan( Count, 0 );

        _cells = cells;
        _fields = fields;
        _fieldOrdinals = null;
    }

    /// <inheritdoc />
    public int Count { get; }

    /// <summary>
    /// Collection of definitions of associated fields.
    /// </summary>
    public ReadOnlySpan<SqlResultSetField> Fields => _fields;

    /// <inheritdoc />
    public SqlQueryReaderRow this[int index]
    {
        get
        {
            Ensure.IsInIndexRange( index, Count );
            return new SqlQueryReaderRow( this, index );
        }
    }

    /// <summary>
    /// Checks whether or not a field with the provided name exists.
    /// </summary>
    /// <param name="fieldName">Name to check.</param>
    /// <returns><b>true</b> when field exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool ContainsField(string fieldName)
    {
        _fieldOrdinals ??= CreateFieldOrdinals( _fields );
        return _fieldOrdinals.ContainsKey( fieldName );
    }

    /// <summary>
    /// Returns an <see cref="SqlResultSetField.Ordinal"/> of a field with the provided name.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <returns>The <see cref="SqlResultSetField.Ordinal"/> of an existing field.</returns>
    /// <exception cref="KeyNotFoundException">When field does not exist.</exception>
    [Pure]
    public int GetOrdinal(string fieldName)
    {
        _fieldOrdinals ??= CreateFieldOrdinals( _fields );
        return _fieldOrdinals[fieldName];
    }

    /// <summary>
    /// Attempts to return an <see cref="SqlResultSetField.Ordinal"/> of a field with the provided name.
    /// </summary>
    /// <param name="fieldName">Field name.</param>
    /// <param name="ordinal"><b>out</b> parameter that returns the <see cref="SqlResultSetField.Ordinal"/> of an existing field.</param>
    /// <returns><b>true</b> when field exists, otherwise <b>false</b>.</returns>
    public bool TryGetOrdinal(string fieldName, out int ordinal)
    {
        _fieldOrdinals ??= CreateFieldOrdinals( _fields );
        return _fieldOrdinals.TryGetValue( fieldName, out ordinal );
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( this );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="SqlQueryReaderRowCollection"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<SqlQueryReaderRow>
    {
        private readonly SqlQueryReaderRowCollection _source;
        private int _index;

        internal Enumerator(SqlQueryReaderRowCollection source)
        {
            _source = source;
            _index = -1;
        }

        /// <inheritdoc />
        public SqlQueryReaderRow Current => new SqlQueryReaderRow( _source, _index );

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return ++_index < _source.Count;
        }

        void IEnumerator.Reset()
        {
            _index = -1;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal object? GetValue(int rowIndex, int ordinal)
    {
        return _cells[rowIndex * _fields.Length + ordinal];
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReadOnlySpan<object?> GetRowSpan(int rowIndex)
    {
        return CollectionsMarshal.AsSpan( _cells ).Slice( rowIndex * _fields.Length, _fields.Length );
    }

    [Pure]
    private static Dictionary<string, int> CreateFieldOrdinals(ReadOnlySpan<SqlResultSetField> fields)
    {
        var result = new Dictionary<string, int>( capacity: fields.Length, comparer: SqlHelpers.NameComparer );
        foreach ( var field in fields )
            result.Add( field.Name, field.Ordinal );

        return result;
    }

    [Pure]
    IEnumerator<SqlQueryReaderRow> IEnumerable<SqlQueryReaderRow>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
