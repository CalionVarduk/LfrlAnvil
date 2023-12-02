using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Sql.Statements;

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

    public int Count { get; }
    public ReadOnlySpan<SqlResultSetField> Fields => _fields;

    public SqlQueryReaderRow this[int index]
    {
        get
        {
            Ensure.IsInIndexRange( index, Count );
            return new SqlQueryReaderRow( this, index );
        }
    }

    [Pure]
    public bool ContainsField(string fieldName)
    {
        _fieldOrdinals ??= CreateFieldOrdinals( _fields );
        return _fieldOrdinals.ContainsKey( fieldName );
    }

    [Pure]
    public int GetOrdinal(string fieldName)
    {
        _fieldOrdinals ??= CreateFieldOrdinals( _fields );
        return _fieldOrdinals[fieldName];
    }

    public bool TryGetOrdinal(string fieldName, out int ordinal)
    {
        _fieldOrdinals ??= CreateFieldOrdinals( _fields );
        return _fieldOrdinals.TryGetValue( fieldName, out ordinal );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( this );
    }

    public struct Enumerator : IEnumerator<SqlQueryReaderRow>
    {
        private readonly SqlQueryReaderRowCollection _source;
        private int _index;

        internal Enumerator(SqlQueryReaderRowCollection source)
        {
            _source = source;
            _index = -1;
        }

        public SqlQueryReaderRow Current => new SqlQueryReaderRow( _source, _index );
        object IEnumerator.Current => Current;

        public void Dispose() { }

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
        var result = new Dictionary<string, int>( capacity: fields.Length, comparer: StringComparer.OrdinalIgnoreCase );
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
