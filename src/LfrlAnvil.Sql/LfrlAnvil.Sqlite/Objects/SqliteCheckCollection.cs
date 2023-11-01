using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteCheckCollection : ISqlCheckCollection
{
    private readonly Dictionary<string, SqliteCheck> _map;

    internal SqliteCheckCollection(SqliteTable table, SqliteCheckBuilderCollection checks)
    {
        Table = table;
        _map = new Dictionary<string, SqliteCheck>( capacity: checks.Count, comparer: StringComparer.OrdinalIgnoreCase );

        foreach ( var b in checks )
        {
            var chk = new SqliteCheck( table, b );
            _map.Add( chk.Name, chk );
        }
    }

    public SqliteTable Table { get; }
    public int Count => _map.Count;

    ISqlTable ISqlCheckCollection.Table => Table;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteCheck Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out SqliteCheck result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteCheck>
    {
        private Dictionary<string, SqliteCheck>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteCheck> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteCheck Current => _enumerator.Current;
        object IEnumerator.Current => Current;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        void IEnumerator.Reset()
        {
            ((IEnumerator)_enumerator).Reset();
        }
    }

    [Pure]
    ISqlCheck ISqlCheckCollection.Get(string name)
    {
        return Get( name );
    }

    bool ISqlCheckCollection.TryGet(string name, [MaybeNullWhen( false )] out ISqlCheck result)
    {
        if ( TryGet( name, out var chk ) )
        {
            result = chk;
            return true;
        }

        result = null;
        return false;
    }

    [Pure]
    IEnumerator<ISqlCheck> IEnumerable<ISqlCheck>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
