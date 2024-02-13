using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public readonly struct SqlDatabaseObjectsSet<T>
    where T : SqlObjectBuilder
{
    private readonly Dictionary<ulong, T> _map;

    private SqlDatabaseObjectsSet(Dictionary<ulong, T> map)
    {
        _map = map;
    }

    public int Count => _map.Count;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseObjectsSet<T> Create()
    {
        return new SqlDatabaseObjectsSet<T>( new Dictionary<ulong, T>() );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(T obj)
    {
        return _map.TryAdd( obj.Id, obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Remove(T obj)
    {
        return _map.Remove( obj.Id );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(T obj)
    {
        return _map.ContainsKey( obj.Id );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Clear()
    {
        _map.Clear();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Dictionary<ulong, T>.ValueCollection.Enumerator GetEnumerator()
    {
        return _map.Values.GetEnumerator();
    }
}
