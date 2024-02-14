using System;
using System.Collections;
using System.Collections.Generic;
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

        using var foreignKeys = builders.Database.ObjectPool.GreedyRent();

        _map = new Dictionary<string, SqliteSchema>( capacity: builders.Count, comparer: StringComparer.OrdinalIgnoreCase );
        foreach ( var b in builders )
        {
            var schema = new SqliteSchema( Database, b );
            schema.Objects.AddConstraintsWithoutForeignKeys( b.Objects, foreignKeys );
            _map.Add( schema.Name, schema );
        }

        Default = _map[builders.Default.Name];
        foreignKeys.Refresh();

        SqliteSchemaBuilder? tableSchemaBuilder = null;
        SqliteSchema? tableSchema = null;

        foreach ( var builder in foreignKeys )
        {
            var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( builder );
            if ( ! ReferenceEquals( tableSchemaBuilder, fk.OriginIndex.Table.Schema ) )
            {
                tableSchemaBuilder = fk.OriginIndex.Table.Schema;
                tableSchema = _map[tableSchemaBuilder.Name];
            }

            Assume.IsNotNull( tableSchema );
            var referencedSchema = ReferenceEquals( tableSchemaBuilder, fk.ReferencedIndex.Table.Schema )
                ? tableSchema
                : _map[fk.ReferencedIndex.Table.Schema.Name];

            tableSchema.Objects.AddForeignKey( fk, referencedSchema );
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

    [Pure]
    public SqliteSchema? TryGet(string name)
    {
        return _map.GetValueOrDefault( name );
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

    [Pure]
    ISqlSchema? ISqlSchemaCollection.TryGet(string name)
    {
        return TryGet( name );
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
