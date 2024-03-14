using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlDatabaseBuilder : SqlDatabaseBuilder
{
    internal MySqlDatabaseBuilder(string serverVersion, string defaultSchemaName, MySqlColumnTypeDefinitionProvider typeDefinitions)
        : base(
            MySqlDialect.Instance,
            serverVersion,
            defaultSchemaName,
            new MySqlDataTypeProvider(),
            typeDefinitions,
            new MySqlNodeInterpreterFactory( typeDefinitions, defaultSchemaName ),
            new MySqlQueryReaderFactory( typeDefinitions ),
            new MySqlParameterBinderFactory( typeDefinitions ),
            new SqlDefaultObjectNameProvider(),
            new MySqlSchemaBuilderCollection(),
            new MySqlDatabaseChangeTracker() )
    {
        CommonSchemaName = defaultSchemaName;
    }

    public string CommonSchemaName { get; }
    public new MySqlSchemaBuilderCollection Schemas => ReinterpretCast.To<MySqlSchemaBuilderCollection>( base.Schemas );
    public new MySqlDataTypeProvider DataTypes => ReinterpretCast.To<MySqlDataTypeProvider>( base.DataTypes );

    public new MySqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<MySqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    public new MySqlNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<MySqlNodeInterpreterFactory>( base.NodeInterpreters );
    public new MySqlQueryReaderFactory QueryReaders => ReinterpretCast.To<MySqlQueryReaderFactory>( base.QueryReaders );
    public new MySqlParameterBinderFactory ParameterBinders => ReinterpretCast.To<MySqlParameterBinderFactory>( base.ParameterBinders );
    public new MySqlDatabaseChangeTracker Changes => ReinterpretCast.To<MySqlDatabaseChangeTracker>( base.Changes );

    public new MySqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        base.AddConnectionChangeCallback( callback );
        return this;
    }

    [Pure]
    public override bool IsValidName(SqlObjectType objectType, string name)
    {
        return base.IsValidName( objectType, name ) && ! name.Contains( '`' );
    }
}
