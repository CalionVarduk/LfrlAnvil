using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public readonly struct SqlDatabaseNamedSchemaObjectsSet<T>
    where T : SqlObjectBuilder
{
    private readonly Dictionary<SqlSchemaObjectName, T> _map;

    private SqlDatabaseNamedSchemaObjectsSet(Dictionary<SqlSchemaObjectName, T> map)
    {
        _map = map;
    }

    public int Count => _map.Count;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseNamedSchemaObjectsSet<T> Create()
    {
        return new SqlDatabaseNamedSchemaObjectsSet<T>( new Dictionary<SqlSchemaObjectName, T>() );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(SqlSchemaObjectName name, T obj)
    {
        return _map.TryAdd( name, obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? Remove(SqlSchemaObjectName name)
    {
        return _map.Remove( name, out var removed ) ? removed : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? TryGetObject(SqlSchemaObjectName name)
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
        private Dictionary<SqlSchemaObjectName, T>.Enumerator _base;

        internal Enumerator(Dictionary<SqlSchemaObjectName, T> map)
        {
            _base = map.GetEnumerator();
        }

        public SqlNamedSchemaObject<T> Current
        {
            get
            {
                var current = _base.Current;
                return new SqlNamedSchemaObject<T>( current.Key, current.Value );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _base.MoveNext();
        }
    }
}
