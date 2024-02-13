using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteDatabaseBuilder : ISqlDatabaseBuilder
{
    private readonly UlongSequenceGenerator _idGenerator;

    internal SqliteDatabaseBuilder(string serverVersion, string defaultSchemaName)
    {
        ServerVersion = serverVersion;
        _idGenerator = new UlongSequenceGenerator();
        DataTypes = new SqliteDataTypeProvider();
        TypeDefinitions = new SqliteColumnTypeDefinitionProvider();
        NodeInterpreters = new SqliteNodeInterpreterFactory( TypeDefinitions );
        QueryReaders = new SqliteQueryReaderFactory( TypeDefinitions );
        ParameterBinders = new SqliteParameterBinderFactory( TypeDefinitions );
        Schemas = new SqliteSchemaBuilderCollection( this, defaultSchemaName );
        Changes = new SqliteDatabaseChangeTracker( this );
        ObjectPool = new MemorySequencePool<SqliteObjectBuilder>( minSegmentLength: 32 );
        ConnectionChanges = SqlDatabaseConnectionChangeCallbacks.Create();
    }

    public SqliteDataTypeProvider DataTypes { get; }
    public SqliteColumnTypeDefinitionProvider TypeDefinitions { get; }
    public SqliteNodeInterpreterFactory NodeInterpreters { get; private set; }
    public SqliteQueryReaderFactory QueryReaders { get; }
    public SqliteParameterBinderFactory ParameterBinders { get; }
    public SqliteSchemaBuilderCollection Schemas { get; }
    public string ServerVersion { get; }
    public SqlDialect Dialect => SqliteDialect.Instance;
    public SqlDatabaseCreateMode Mode => Changes.Mode;
    public bool IsAttached => Changes.IsAttached;
    public SqliteDatabaseChangeTracker Changes { get; }
    internal MemorySequencePool<SqliteObjectBuilder> ObjectPool { get; }
    internal SqlDatabaseConnectionChangeCallbacks ConnectionChanges { get; private set; }
    ISqlDataTypeProvider ISqlDatabaseBuilder.DataTypes => DataTypes;
    ISqlColumnTypeDefinitionProvider ISqlDatabaseBuilder.TypeDefinitions => TypeDefinitions;
    ISqlNodeInterpreterFactory ISqlDatabaseBuilder.NodeInterpreters => NodeInterpreters;
    ISqlQueryReaderFactory ISqlDatabaseBuilder.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabaseBuilder.ParameterBinders => ParameterBinders;
    ISqlSchemaBuilderCollection ISqlDatabaseBuilder.Schemas => Schemas;
    ISqlDatabaseChangeTracker ISqlDatabaseBuilder.Changes => Changes;

    public SqliteDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        ConnectionChanges.AddCallback( callback );
        return this;
    }

    [Pure]
    internal ReadOnlySpan<Action<SqlDatabaseConnectionChangeEvent>> GetPendingConnectionChangeCallbacks()
    {
        var result = ConnectionChanges.GetPendingCallbacks();
        ConnectionChanges = ConnectionChanges.UpdateFirstPendingCallbackIndex();
        return result;
    }

    internal ulong GetNextId()
    {
        return _idGenerator.Generate();
    }

    internal static void RemoveReferencingForeignKeys(SqliteTableBuilder table, RentedMemorySequenceSpan<SqliteObjectBuilder> foreignKeys)
    {
        if ( foreignKeys.Length == 0 )
            return;

        PrepareReferencingForeignKeysForRemoval( foreignKeys, table.Database.Changes.ActiveObject, table );

        foreach ( var obj in foreignKeys )
        {
            var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( obj );
            Assume.Equals( table, fk.ReferencedIndex.Table );
            if ( ! fk.IsSelfReference() )
                fk.Remove();
        }
    }

    private static void PrepareReferencingForeignKeysForRemoval(
        RentedMemorySequenceSpan<SqliteObjectBuilder> foreignKeys,
        SqliteObjectBuilder? objectWithOngoingChanges = null,
        SqliteTableBuilder? sourceTable = null)
    {
        Assume.IsGreaterThan( foreignKeys.Length, 0 );

        if ( objectWithOngoingChanges is not null && ! ReferenceEquals( objectWithOngoingChanges, sourceTable ) )
        {
            var i = 0;
            while ( i < foreignKeys.Length )
            {
                var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( foreignKeys[i] );
                if ( ! ReferenceEquals( fk.OriginIndex.Table, objectWithOngoingChanges ) )
                    break;

                ++i;
            }

            var j = i++;
            while ( i < foreignKeys.Length )
            {
                var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( foreignKeys[i] );
                if ( ReferenceEquals( fk.OriginIndex.Table, objectWithOngoingChanges ) )
                {
                    foreignKeys[i] = foreignKeys[j];
                    foreignKeys[j++] = fk;
                }

                ++i;
            }

            foreignKeys = foreignKeys.Slice( j );
        }

        foreignKeys.Sort(
            static (a, b) =>
            {
                var fk1 = ReinterpretCast.To<SqliteForeignKeyBuilder>( a );
                var fk2 = ReinterpretCast.To<SqliteForeignKeyBuilder>( b );
                return fk1.OriginIndex.Table.Id.CompareTo( fk2.OriginIndex.Table.Id );
            } );
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        return AddConnectionChangeCallback( callback );
    }
}
