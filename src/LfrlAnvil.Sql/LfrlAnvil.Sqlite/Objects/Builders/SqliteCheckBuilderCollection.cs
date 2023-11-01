using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteCheckBuilderCollection : ISqlCheckBuilderCollection
{
    private readonly Dictionary<string, SqliteCheckBuilder> _map;

    internal SqliteCheckBuilderCollection(SqliteTableBuilder table)
    {
        Table = table;
        _map = new Dictionary<string, SqliteCheckBuilder>( comparer: StringComparer.OrdinalIgnoreCase );
    }

    public SqliteTableBuilder Table { get; }
    public int Count => _map.Count;

    ISqlTableBuilder ISqlCheckBuilderCollection.Table => Table;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteCheckBuilder Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out SqliteCheckBuilder result)
    {
        return _map.TryGetValue( name, out result );
    }

    public SqliteCheckBuilder Create(SqlConditionNode condition)
    {
        Table.EnsureNotRemoved();

        var name = SqliteHelpers.GetDefaultCheckName( Table );
        if ( _map.ContainsKey( name ) )
            throw new SqliteObjectBuilderException( ExceptionResources.CheckAlreadyExists( name ) );

        var check = Table.Schema.Objects.CreateCheck( Table, name, condition );
        _map.Add( name, check );
        return check;
    }

    public bool Remove(string name)
    {
        if ( ! _map.Remove( name, out var removed ) )
            return false;

        removed.Remove();
        return true;
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteCheckBuilder>
    {
        private Dictionary<string, SqliteCheckBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteCheckBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteCheckBuilder Current => _enumerator.Current;
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

    internal void ChangeName(SqliteCheckBuilder check, string name)
    {
        _map.Add( name, check );
        _map.Remove( check.Name );
    }

    internal void ClearInto(RentedMemorySequenceSpan<SqliteObjectBuilder> buffer)
    {
        _map.Values.CopyTo( buffer );
        _map.Clear();
    }

    [Pure]
    ISqlCheckBuilder ISqlCheckBuilderCollection.Get(string name)
    {
        return Get( name );
    }

    bool ISqlCheckBuilderCollection.TryGet(string name, [MaybeNullWhen( false )] out ISqlCheckBuilder result)
    {
        if ( TryGet( name, out var chk ) )
        {
            result = chk;
            return true;
        }

        result = null;
        return false;
    }

    ISqlCheckBuilder ISqlCheckBuilderCollection.Create(SqlConditionNode condition)
    {
        return Create( condition );
    }

    [Pure]
    IEnumerator<ISqlCheckBuilder> IEnumerable<ISqlCheckBuilder>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
