using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sqlite.Exceptions;
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
        NodeInterpreters = new SqliteNodeInterpreterFactory( TypeDefinitions );
        QueryReaders = new SqliteQueryReaderFactory( TypeDefinitions );
        ParameterBinders = new SqliteParameterBinderFactory( TypeDefinitions );
        Schemas = new SqliteSchemaBuilderCollection( this );
        ChangeTracker = new SqliteDatabaseChangeTracker( this );
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
    public SqlDatabaseCreateMode Mode => ChangeTracker.Mode;
    public bool IsAttached => ChangeTracker.IsAttached;
    internal SqliteDatabaseChangeTracker ChangeTracker { get; }
    internal MemorySequencePool<SqliteObjectBuilder> ObjectPool { get; }
    internal SqlDatabaseConnectionChangeCallbacks ConnectionChanges { get; private set; }
    ISqlDataTypeProvider ISqlDatabaseBuilder.DataTypes => DataTypes;
    ISqlColumnTypeDefinitionProvider ISqlDatabaseBuilder.TypeDefinitions => TypeDefinitions;
    ISqlNodeInterpreterFactory ISqlDatabaseBuilder.NodeInterpreters => NodeInterpreters;
    ISqlQueryReaderFactory ISqlDatabaseBuilder.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabaseBuilder.ParameterBinders => ParameterBinders;
    ISqlSchemaBuilderCollection ISqlDatabaseBuilder.Schemas => Schemas;

    public ReadOnlySpan<SqlDatabaseBuilderStatement> GetPendingStatements()
    {
        return ChangeTracker.GetPendingStatements();
    }

    public void AddStatement(ISqlStatementNode statement)
    {
        var context = NodeInterpreters.Create().Interpret( statement.Node );
        if ( context.Parameters.Count > 0 )
            throw new SqliteObjectBuilderException( Chain.Create( ExceptionResources.StatementIsParameterized( statement, context ) ) );

        ChangeTracker.AddStatement( SqlDatabaseBuilderStatement.Create( context ) );
    }

    public void AddParameterizedStatement(
        ISqlStatementNode statement,
        IEnumerable<KeyValuePair<string, object?>> parameters,
        SqlParameterBinderCreationOptions? options = null)
    {
        var context = NodeInterpreters.Create().Interpret( statement.Node );
        var opt = options ?? SqlParameterBinderCreationOptions.Default;
        var executor = ParameterBinders.Create( opt.SetContext( context ) ).Bind( parameters );
        ChangeTracker.AddStatement( SqlDatabaseBuilderStatement.Create( context, executor ) );
    }

    public void AddParameterizedStatement<TSource>(
        ISqlStatementNode statement,
        TSource parameters,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull
    {
        var context = NodeInterpreters.Create().Interpret( statement.Node );
        var opt = options ?? SqlParameterBinderCreationOptions.Default;
        var executor = ParameterBinders.Create<TSource>( opt.SetContext( context ) ).Bind( parameters );
        ChangeTracker.AddStatement( SqlDatabaseBuilderStatement.Create( context, executor ) );
    }

    public SqliteDatabaseBuilder SetNodeInterpreterFactory(SqliteNodeInterpreterFactory factory)
    {
        NodeInterpreters = factory;
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
