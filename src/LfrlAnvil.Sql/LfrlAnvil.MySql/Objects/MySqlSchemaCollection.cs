using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlSchemaCollection : ISqlSchemaCollection
{
    private readonly Dictionary<string, MySqlSchema> _map;

    internal MySqlSchemaCollection(MySqlDatabase database, MySqlSchemaBuilderCollection builders)
    {
        Database = database;

        using var tables = builders.Database.ObjectPool.GreedyRent();

        _map = new Dictionary<string, MySqlSchema>( capacity: builders.Count, comparer: StringComparer.OrdinalIgnoreCase );
        foreach ( var b in builders )
            _map.Add( b.Name, MySqlSchema.Create( database, b, tables ) );

        Default = _map[builders.Default.Name];

        tables.Refresh();
        MySqlSchemaBuilder? tableSchemaBuilder = null;
        MySqlSchema? tableSchema = null;

        foreach ( var o in tables )
        {
            var tableBuilder = ReinterpretCast.To<MySqlTableBuilder>( o );
            if ( ! ReferenceEquals( tableSchemaBuilder, tableBuilder.Schema ) )
            {
                tableSchemaBuilder = tableBuilder.Schema;
                tableSchema = _map[tableSchemaBuilder.Name];
            }

            Assume.IsNotNull( tableSchema );
            var table = tableSchema.Objects.GetTable( tableBuilder.Name );
            table.ForeignKeys.Populate( this, tableBuilder.ForeignKeys );
            tableSchema.Objects.Populate( table.ForeignKeys );
        }
    }

    public MySqlDatabase Database { get; }
    public MySqlSchema Default { get; }
    public int Count => _map.Count;

    ISqlSchema ISqlSchemaCollection.Default => Default;
    ISqlDatabase ISqlSchemaCollection.Database => Database;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public MySqlSchema Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out MySqlSchema result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<MySqlSchema>
    {
        private Dictionary<string, MySqlSchema>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, MySqlSchema> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public MySqlSchema Current => _enumerator.Current;
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
    ISqlSchema ISqlSchemaCollection.Get(string name)
    {
        return Get( name );
    }

    bool ISqlSchemaCollection.TryGet(string name, [MaybeNullWhen( false )] out ISqlSchema result)
    {
        if ( TryGet( name, out var schema ) )
        {
            result = schema;
            return true;
        }

        result = null;
        return false;
    }

    [Pure]
    IEnumerator<ISqlSchema> IEnumerable<ISqlSchema>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
