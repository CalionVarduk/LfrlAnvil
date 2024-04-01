using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql;

public readonly struct PostgreSqlDatabaseFactoryOptions
{
    public static readonly PostgreSqlDatabaseFactoryOptions Default = new PostgreSqlDatabaseFactoryOptions();

    public static readonly SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>
        BaseTypeDefinitionsCreator = static (_, _) => new PostgreSqlColumnTypeDefinitionProviderBuilder().Build();

    public static readonly
        SqlNodeInterpreterFactoryCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider,
            PostgreSqlNodeInterpreterFactory>
        BaseNodeInterpretersCreator = static (_, _, _, typeDefinitions) =>
            new PostgreSqlNodeInterpreterFactory( PostgreSqlNodeInterpreterOptions.Default.SetTypeDefinitions( typeDefinitions ) );

    private readonly SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? _defaultNamesCreator;

    private readonly SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>?
        _typeDefinitionsCreator;

    private readonly
        SqlNodeInterpreterFactoryCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider,
            PostgreSqlNodeInterpreterFactory>?
        _nodeInterpretersCreator;

    private PostgreSqlDatabaseFactoryOptions(
        SqlOptionalFunctionalityResolution virtualGeneratedColumnStorageResolution,
        SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? defaultNamesCreator,
        SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>? typeDefinitionsCreator,
        SqlNodeInterpreterFactoryCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider,
                PostgreSqlNodeInterpreterFactory>?
            nodeInterpretersCreator)
    {
        VirtualGeneratedColumnStorageResolution = virtualGeneratedColumnStorageResolution;
        _defaultNamesCreator = defaultNamesCreator;
        _typeDefinitionsCreator = typeDefinitionsCreator;
        _nodeInterpretersCreator = nodeInterpretersCreator;
    }

    public SqlOptionalFunctionalityResolution VirtualGeneratedColumnStorageResolution { get; }

    public SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider> DefaultNamesCreator =>
        _defaultNamesCreator ?? SqlHelpers.DefaultNamesCreator;

    public SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>
        TypeDefinitionsCreator =>
        _typeDefinitionsCreator ?? BaseTypeDefinitionsCreator;

    public SqlNodeInterpreterFactoryCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider,
            PostgreSqlNodeInterpreterFactory>
        NodeInterpretersCreator =>
        _nodeInterpretersCreator ?? BaseNodeInterpretersCreator;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlDatabaseFactoryOptions SetVirtualGeneratedColumnStorageResolution(SqlOptionalFunctionalityResolution resolution)
    {
        Ensure.IsDefined( resolution );
        return new PostgreSqlDatabaseFactoryOptions( resolution, _defaultNamesCreator, _typeDefinitionsCreator, _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlDatabaseFactoryOptions SetDefaultNamesCreator(
        SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? creator)
    {
        return new PostgreSqlDatabaseFactoryOptions(
            VirtualGeneratedColumnStorageResolution,
            creator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlDatabaseFactoryOptions SetTypeDefinitionsCreator(
        SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>? creator)
    {
        return new PostgreSqlDatabaseFactoryOptions(
            VirtualGeneratedColumnStorageResolution,
            _defaultNamesCreator,
            creator,
            _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlDatabaseFactoryOptions SetNodeInterpretersCreator(
        SqlNodeInterpreterFactoryCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider,
            PostgreSqlNodeInterpreterFactory>? creator)
    {
        return new PostgreSqlDatabaseFactoryOptions(
            VirtualGeneratedColumnStorageResolution,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            creator );
    }
}
