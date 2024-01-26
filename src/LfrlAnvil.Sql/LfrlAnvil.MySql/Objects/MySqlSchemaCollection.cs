using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlSchemaCollection : ISqlSchemaCollection
{
    private readonly Dictionary<string, MySqlSchema> _map;

    internal MySqlSchemaCollection(MySqlDatabase database, MySqlSchemaBuilderCollection builders)
    {
        Database = database;

        using var foreignKeys = builders.Database.ObjectPool.GreedyRent();

        _map = new Dictionary<string, MySqlSchema>( capacity: builders.Count, comparer: StringComparer.OrdinalIgnoreCase );
        foreach ( var b in builders )
        {
            var schema = new MySqlSchema( Database, b );
            schema.Objects.AddConstraintsWithoutForeignKeys( b.Objects, foreignKeys );
            _map.Add( schema.Name, schema );
        }

        Default = _map[builders.Default.Name];
        foreignKeys.Refresh();

        MySqlSchemaBuilder? tableSchemaBuilder = null;
        MySqlSchema? tableSchema = null;

        foreach ( var builder in foreignKeys )
        {
            var fk = ReinterpretCast.To<MySqlForeignKeyBuilder>( builder );
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
    public MySqlSchema GetSchema(string name)
    {
        return _map[name];
    }

    [Pure]
    public MySqlSchema? TryGetSchema(string name)
    {
        return _map.GetValueOrDefault( name );
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
    ISqlSchema ISqlSchemaCollection.GetSchema(string name)
    {
        return GetSchema( name );
    }

    [Pure]
    ISqlSchema? ISqlSchemaCollection.TryGetSchema(string name)
    {
        return TryGetSchema( name );
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
