using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteSchemaCollection : ISqlSchemaCollection
{
    private readonly Dictionary<string, SqliteSchema> _map;

    internal SqliteSchemaCollection(SqliteDatabase database, SqliteSchemaBuilderCollection builders)
    {
        Database = database;

        using var tables = builders.Database.ObjectPool.GreedyRent();

        _map = new Dictionary<string, SqliteSchema>( capacity: builders.Count, comparer: StringComparer.OrdinalIgnoreCase );
        foreach ( var b in builders )
            _map.Add( b.Name, SqliteSchema.Create( database, b, tables ) );

        Default = _map[builders.Default.Name];

        tables.Refresh();
        SqliteSchemaBuilder? tableSchemaBuilder = null;
        SqliteSchema? tableSchema = null;

        foreach ( var o in tables )
        {
            var tableBuilder = ReinterpretCast.To<SqliteTableBuilder>( o );
            if ( ! ReferenceEquals( tableSchemaBuilder, tableBuilder.Schema ) )
            {
                tableSchemaBuilder = tableBuilder.Schema;
                tableSchema = _map[tableSchemaBuilder.Name];
            }

            Assume.IsNotNull( tableSchema, nameof( tableSchema ) );
            var table = tableSchema.Objects.GetTable( tableBuilder.Name );
            table.ForeignKeys.Populate( this, tableBuilder.ForeignKeys );
            tableSchema.Objects.Populate( table.ForeignKeys );
        }
    }

    public SqliteDatabase Database { get; }
    public SqliteSchema Default { get; }
    public int Count => _map.Count;

    ISqlSchema ISqlSchemaCollection.Default => Default;
    ISqlDatabase ISqlSchemaCollection.Database => Database;

    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    [Pure]
    public SqliteSchema Get(string name)
    {
        return _map[name];
    }

    public bool TryGet(string name, [MaybeNullWhen( false )] out SqliteSchema result)
    {
        return _map.TryGetValue( name, out result );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    public struct Enumerator : IEnumerator<SqliteSchema>
    {
        private Dictionary<string, SqliteSchema>.ValueCollection.Enumerator _enumerator;

        internal Enumerator(Dictionary<string, SqliteSchema> source)
        {
            _enumerator = source.Values.GetEnumerator();
        }

        public SqliteSchema Current => _enumerator.Current;
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
