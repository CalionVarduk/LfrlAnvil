using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteDatabaseBuilder : ISqlDatabaseBuilder
{
    private readonly UlongSequenceGenerator _idGenerator;

    internal SqliteDatabaseBuilder(string serverVersion)
    {
        ServerVersion = serverVersion;
        _idGenerator = new UlongSequenceGenerator();
        DataTypes = new SqliteDataTypeProvider();
        TypeDefinitions = new SqliteColumnTypeDefinitionProvider();
        NodeInterpreterFactory = new SqliteNodeInterpreterFactory( TypeDefinitions );
        Schemas = new SqliteSchemaBuilderCollection( this );
        ChangeTracker = new SqliteDatabaseChangeTracker( this );
        ObjectPool = new MemorySequencePool<SqliteObjectBuilder>( minSegmentLength: 32 );
        ConnectionChanges = SqlDatabaseConnectionChangeCallbacks.Create();
    }

    public SqliteDataTypeProvider DataTypes { get; }
    public SqliteColumnTypeDefinitionProvider TypeDefinitions { get; }
    public SqliteNodeInterpreterFactory NodeInterpreterFactory { get; private set; }
    public SqliteSchemaBuilderCollection Schemas { get; }
    public string ServerVersion { get; }
    public SqlDialect Dialect => SqliteDialect.Instance;
    public SqlDatabaseCreateMode Mode => ChangeTracker.Mode;
    public bool IsAttached => ChangeTracker.IsAttached;
    internal SqliteDatabaseChangeTracker ChangeTracker { get; }
    internal MemorySequencePool<SqliteObjectBuilder> ObjectPool { get; }
    internal SqlDatabaseConnectionChangeCallbacks ConnectionChanges { get; private set; }
    ISqlDataTypeProvider ISqlDatabaseBuilder.DataTypes => DataTypes;
    ISqlColumnTypeDefinitionProvider ISqlDatabaseBuilder.TypeDefinitions => TypeDefinitions;
    ISqlNodeInterpreterFactory ISqlDatabaseBuilder.NodeInterpreterFactory => NodeInterpreterFactory;
    ISqlSchemaBuilderCollection ISqlDatabaseBuilder.Schemas => Schemas;

    public ReadOnlySpan<string> GetPendingStatements()
    {
        return ChangeTracker.GetPendingStatements();
    }

    public void AddRawStatement(string statement)
    {
        ChangeTracker.AddRawStatement( statement );
    }

    public SqliteDatabaseBuilder SetNodeInterpreterFactory(SqliteNodeInterpreterFactory factory)
    {
        NodeInterpreterFactory = factory;
        return this;
    }

    public SqliteDatabaseBuilder SetAttachedMode(bool enabled = true)
    {
        ChangeTracker.SetAttachedMode( enabled );
        return this;
    }

    public SqliteDatabaseBuilder SetDetachedMode(bool enabled = true)
    {
        ChangeTracker.SetAttachedMode( ! enabled );
        return this;
    }

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

        PrepareReferencingForeignKeysForRemoval( foreignKeys, table.Database.ChangeTracker.CurrentObject, table );

        foreach ( var obj in foreignKeys )
        {
            var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( obj );
            Assume.Equals( table, fk.ReferencedIndex.Table, nameof( table ) );
            if ( ! fk.IsSelfReference() )
                fk.Remove();
        }
    }

    private static void PrepareReferencingForeignKeysForRemoval(
        RentedMemorySequenceSpan<SqliteObjectBuilder> foreignKeys,
        SqliteObjectBuilder? objectWithOngoingChanges = null,
        SqliteTableBuilder? sourceTable = null)
    {
        Assume.IsGreaterThan( foreignKeys.Length, 0, nameof( foreignKeys.Length ) );

        if ( objectWithOngoingChanges is not null && ! ReferenceEquals( objectWithOngoingChanges, sourceTable ) )
        {
            var i = 0;
            while ( i < foreignKeys.Length )
            {
                var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( foreignKeys[i] );
                if ( ! ReferenceEquals( fk.Index.Table, objectWithOngoingChanges ) )
                    break;

                ++i;
            }

            var j = i++;
            while ( i < foreignKeys.Length )
            {
                var fk = ReinterpretCast.To<SqliteForeignKeyBuilder>( foreignKeys[i] );
                if ( ReferenceEquals( fk.Index.Table, objectWithOngoingChanges ) )
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
                return fk1.Index.Table.Id.CompareTo( fk2.Index.Table.Id );
            } );
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.SetNodeInterpreterFactory(ISqlNodeInterpreterFactory factory)
    {
        return SetNodeInterpreterFactory( SqliteHelpers.CastOrThrow<SqliteNodeInterpreterFactory>( factory ) );
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.SetDetachedMode(bool enabled)
    {
        return SetDetachedMode( enabled );
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.SetAttachedMode(bool enabled)
    {
        return SetAttachedMode( enabled );
    }

    ISqlDatabaseBuilder ISqlDatabaseBuilder.AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        return AddConnectionChangeCallback( callback );
    }
}
