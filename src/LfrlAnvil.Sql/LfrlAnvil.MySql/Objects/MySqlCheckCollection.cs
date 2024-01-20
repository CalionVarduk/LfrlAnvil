using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlCheckCollection : ISqlCheckCollection
{
    private readonly Dictionary<string, MySqlCheck> _map;

    internal MySqlCheckCollection(MySqlTable table, MySqlCheckBuilderCollection checks)
    {
        Table = table;
        _map = new Dictionary<string, MySqlCheck>( capacity: checks.Count, comparer: StringComparer.OrdinalIgnoreCase );

        foreach ( var b in checks )
        {
            var chk = new MySqlCheck( table, b );
            _map.Add( chk.Name, chk );
        }
    }

    public MySqlTable Table { get; }
    public int Count => _map.Count;

    ISqlTable ISqlCheckCollection.Table => Table;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlCheck Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out MySqlCheck result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlCheck>
    {
        private Dictionary<string, MySqlCheck>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlCheck> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlCheck Current => _enumerator.Current;
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
