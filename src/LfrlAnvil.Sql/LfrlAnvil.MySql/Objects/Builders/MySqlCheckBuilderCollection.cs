using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlCheckBuilderCollection : ISqlCheckBuilderCollection
{
    private readonly Dictionary<string, MySqlCheckBuilder> _map;

    internal MySqlCheckBuilderCollection(MySqlTableBuilder table)
    {
        Table = table;
        _map = new Dictionary<string, MySqlCheckBuilder>( comparer: StringComparer.OrdinalIgnoreCase );
    }

    public MySqlTableBuilder Table { get; }
    public int Count => _map.Count;

    ISqlTableBuilder ISqlCheckBuilderCollection.Table => Table;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlCheckBuilder Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out MySqlCheckBuilder result)
    {
        return _map.TryGetValue( name, out result );
    }

    public MySqlCheckBuilder Create(SqlConditionNode condition)
    {
        Table.EnsureNotRemoved();

        var name = MySqlHelpers.GetDefaultCheckName( Table );
        if ( _map.ContainsKey( name ) )
            throw new MySqlObjectBuilderException( ExceptionResources.CheckAlreadyExists( name ) );

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

    public struct Enumerator : IEnumerator<MySqlCheckBuilder>
    {
        private Dictionary<string, MySqlCheckBuilder>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlCheckBuilder> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlCheckBuilder Current => _enumerator.Current;
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

    internal void ChangeName(MySqlCheckBuilder check, string name)
    {
        _map.Add( name, check );
        _map.Remove( check.Name );
    }

    internal void ClearInto(RentedMemorySequenceSpan<MySqlObjectBuilder> buffer)
    {
        _map.Values.CopyTo( buffer );
        _map.Clear();
    }

    internal void Clear()
    {
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
