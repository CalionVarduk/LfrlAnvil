using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteDatabaseBuilder : SqlDatabaseBuilder
{
    internal SqliteDatabaseBuilder(
        string serverVersion,
        string defaultSchemaName,
        SqlDefaultObjectNameProvider defaultNames,
        SqliteDataTypeProvider dataTypes,
        SqliteColumnTypeDefinitionProvider typeDefinitions,
        SqliteNodeInterpreterFactory nodeInterpreters)
        : base(
            SqliteDialect.Instance,
            serverVersion,
            defaultSchemaName,
            dataTypes,
            typeDefinitions,
            nodeInterpreters,
            new SqliteQueryReaderFactory( typeDefinitions ),
            new SqliteParameterBinderFactory( typeDefinitions, nodeInterpreters.Options.ArePositionalParametersEnabled ),
            defaultNames,
            new SqliteSchemaBuilderCollection(),
            new SqliteDatabaseChangeTracker() ) { }

    public new SqliteSchemaBuilderCollection Schemas => ReinterpretCast.To<SqliteSchemaBuilderCollection>( base.Schemas );
    public new SqliteDataTypeProvider DataTypes => ReinterpretCast.To<SqliteDataTypeProvider>( base.DataTypes );

    public new SqliteColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<SqliteColumnTypeDefinitionProvider>( base.TypeDefinitions );

    public new SqliteNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<SqliteNodeInterpreterFactory>( base.NodeInterpreters );
    public new SqliteQueryReaderFactory QueryReaders => ReinterpretCast.To<SqliteQueryReaderFactory>( base.QueryReaders );
    public new SqliteParameterBinderFactory ParameterBinders => ReinterpretCast.To<SqliteParameterBinderFactory>( base.ParameterBinders );
    public new SqliteDatabaseChangeTracker Changes => ReinterpretCast.To<SqliteDatabaseChangeTracker>( base.Changes );

    public new SqliteDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        base.AddConnectionChangeCallback( callback );
        return this;
    }

    [Pure]
    public override bool IsValidName(SqlObjectType objectType, string name)
    {
        if ( objectType == SqlObjectType.Schema && name.Length == 0 )
            return true;

        return base.IsValidName( objectType, name ) && ! name.Contains( '"' );
    }
}
