using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlDatabaseBuilder : SqlDatabaseBuilder
{
    internal MySqlDatabaseBuilder(
        string serverVersion,
        string defaultSchemaName,
        SqlDefaultObjectNameProvider defaultNames,
        MySqlDataTypeProvider dataTypes,
        MySqlColumnTypeDefinitionProvider typeDefinitions,
        MySqlNodeInterpreterFactory nodeInterpreters,
        SqlOptionalFunctionalityResolution indexFilterResolution,
        string? characterSetName,
        string? collationName,
        bool? isEncryptionEnabled)
        : base(
            MySqlDialect.Instance,
            serverVersion,
            defaultSchemaName,
            dataTypes,
            typeDefinitions,
            nodeInterpreters,
            new MySqlQueryReaderFactory( typeDefinitions ),
            new MySqlParameterBinderFactory( typeDefinitions ),
            defaultNames,
            new MySqlSchemaBuilderCollection(),
            new MySqlDatabaseChangeTracker() )
    {
        Assume.IsDefined( indexFilterResolution );
        CommonSchemaName = defaultSchemaName;
        IndexFilterResolution = indexFilterResolution;
        CharacterSetName = characterSetName;
        CollationName = collationName;
        IsEncryptionEnabled = isEncryptionEnabled;
    }

    public string CommonSchemaName { get; }
    public SqlOptionalFunctionalityResolution IndexFilterResolution { get; }
    public string? CharacterSetName { get; }
    public string? CollationName { get; }
    public bool? IsEncryptionEnabled { get; }
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
