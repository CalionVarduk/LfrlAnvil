using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public readonly struct SqlColumnModificationSourcesSet<T>
    where T : SqlColumnBuilder
{
    private readonly Dictionary<ulong, SqlColumnModificationSource<T>> _map;

    private SqlColumnModificationSourcesSet(Dictionary<ulong, SqlColumnModificationSource<T>> map)
    {
        _map = map;
    }

    public int Count => _map.Count;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnModificationSourcesSet<T> Create()
    {
        return new SqlColumnModificationSourcesSet<T>( new Dictionary<ulong, SqlColumnModificationSource<T>>() );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(SqlColumnModificationSource<T> source)
    {
        return _map.TryAdd( source.Column.Id, source );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(T column)
    {
        return Add( SqlColumnModificationSource<T>.Self( column ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlColumnModificationSource<T>? Remove(T column)
    {
        return _map.Remove( column.Id, out var removed ) ? removed : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlColumnModificationSource<T>? TryGetSource(T column)
    {
        return _map.TryGetValue( column.Id, out var value ) ? value : null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Clear()
    {
        _map.Clear();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator
    {
        private Dictionary<ulong, SqlColumnModificationSource<T>>.ValueCollection.Enumerator _base;

        internal Enumerator(Dictionary<ulong, SqlColumnModificationSource<T>> map)
        {
            _base = map.Values.GetEnumerator();
        }

        public SqlColumnModificationSource<T> Current => _base.Current;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _base.MoveNext();
        }
    }
}
