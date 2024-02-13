using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public readonly struct SqlDatabaseNamedObjectsSet<T>
    where T : SqlObjectBuilder
{
    private readonly Dictionary<string, T> _map;

    private SqlDatabaseNamedObjectsSet(Dictionary<string, T> map)
    {
        _map = map;
    }

    public int Count => _map.Count;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseNamedObjectsSet<T> Create()
    {
        return new SqlDatabaseNamedObjectsSet<T>( new Dictionary<string, T>( SqlHelpers.NameComparer ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(string name, T obj)
    {
        return _map.TryAdd( name, obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Remove(string name)
    {
        return _map.Remove( name );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? TryGetObject(string name)
    {
        return _map.GetValueOrDefault( name );
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
        private Dictionary<string, T>.Enumerator _base;

        internal Enumerator(Dictionary<string, T> map)
        {
            _base = map.GetEnumerator();
        }

        public SqlNamedObject<T> Current
        {
            get
            {
                var current = _base.Current;
                return new SqlNamedObject<T>( current.Key, current.Value );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _base.MoveNext();
        }
    }
}
