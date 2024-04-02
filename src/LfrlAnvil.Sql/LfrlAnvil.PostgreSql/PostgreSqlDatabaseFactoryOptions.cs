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
        string? encodingName,
        string? localeName,
        int? concurrentConnectionsLimit,
        SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? defaultNamesCreator,
        SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>? typeDefinitionsCreator,
        SqlNodeInterpreterFactoryCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider,
                PostgreSqlNodeInterpreterFactory>?
            nodeInterpretersCreator)
    {
        VirtualGeneratedColumnStorageResolution = virtualGeneratedColumnStorageResolution;
        EncodingName = encodingName;
        LocaleName = localeName;
        ConcurrentConnectionsLimit = concurrentConnectionsLimit;
        _defaultNamesCreator = defaultNamesCreator;
        _typeDefinitionsCreator = typeDefinitionsCreator;
        _nodeInterpretersCreator = nodeInterpretersCreator;
    }

    public SqlOptionalFunctionalityResolution VirtualGeneratedColumnStorageResolution { get; }
    public string? EncodingName { get; }
    public string? LocaleName { get; }
    public int? ConcurrentConnectionsLimit { get; }

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
        return new PostgreSqlDatabaseFactoryOptions(
            resolution,
            EncodingName,
            LocaleName,
            ConcurrentConnectionsLimit,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlDatabaseFactoryOptions SetEncodingName(string? name)
    {
        return new PostgreSqlDatabaseFactoryOptions(
            VirtualGeneratedColumnStorageResolution,
            name,
            LocaleName,
            ConcurrentConnectionsLimit,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlDatabaseFactoryOptions SetLocaleName(string? name)
    {
        return new PostgreSqlDatabaseFactoryOptions(
            VirtualGeneratedColumnStorageResolution,
            EncodingName,
            name,
            ConcurrentConnectionsLimit,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlDatabaseFactoryOptions SetConcurrentConnectionsLimit(int? value)
    {
        if ( value is not null )
            Ensure.IsGreaterThanOrEqualTo( value.Value, 0 );

        return new PostgreSqlDatabaseFactoryOptions(
            VirtualGeneratedColumnStorageResolution,
            EncodingName,
            LocaleName,
            value,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public PostgreSqlDatabaseFactoryOptions SetDefaultNamesCreator(
        SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? creator)
    {
        return new PostgreSqlDatabaseFactoryOptions(
            VirtualGeneratedColumnStorageResolution,
            EncodingName,
            LocaleName,
            ConcurrentConnectionsLimit,
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
            EncodingName,
            LocaleName,
            ConcurrentConnectionsLimit,
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
            EncodingName,
            LocaleName,
            ConcurrentConnectionsLimit,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            creator );
    }
}
