using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql;

/// <summary>
/// Represents available options for creating PostgreSQL database objects through <see cref="PostgreSqlDatabaseFactory"/>.
/// </summary>
public readonly struct PostgreSqlDatabaseFactoryOptions
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static readonly PostgreSqlDatabaseFactoryOptions Default = new PostgreSqlDatabaseFactoryOptions();

    /// <summary>
    /// Default creator of <see cref="PostgreSqlColumnTypeDefinitionProvider"/> instances.
    /// </summary>
    public static readonly SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>
        BaseTypeDefinitionsCreator = static (_, _) => new PostgreSqlColumnTypeDefinitionProviderBuilder().Build();

    /// <summary>
    /// Default creator of <see cref="PostgreSqlNodeInterpreterFactory"/> instances.
    /// </summary>
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

    /// <summary>
    /// Specifies how virtual computed columns should be resolved by DB factory.
    /// </summary>
    public SqlOptionalFunctionalityResolution VirtualGeneratedColumnStorageResolution { get; }

    /// <summary>
    /// Specifies DB encoding.
    /// </summary>
    public string? EncodingName { get; }

    /// <summary>
    /// Specifies DB locale.
    /// </summary>
    public string? LocaleName { get; }

    /// <summary>
    /// Specifies maximum concurrent connections to DB.
    /// </summary>
    public int? ConcurrentConnectionsLimit { get; }

    /// <summary>
    /// Specifies the creator of <see cref="SqlDefaultObjectNameProvider"/> instances.
    /// </summary>
    public SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider> DefaultNamesCreator =>
        _defaultNamesCreator ?? SqlHelpers.DefaultNamesCreator;

    /// <summary>
    /// Specifies the creator of <see cref="PostgreSqlColumnTypeDefinitionProvider"/> instances.
    /// </summary>
    public SqlColumnTypeDefinitionProviderCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider>
        TypeDefinitionsCreator =>
        _typeDefinitionsCreator ?? BaseTypeDefinitionsCreator;

    /// <summary>
    /// Specifies the creator of <see cref="PostgreSqlNodeInterpreterFactory"/> instances.
    /// </summary>
    public SqlNodeInterpreterFactoryCreator<PostgreSqlDataTypeProvider, PostgreSqlColumnTypeDefinitionProvider,
            PostgreSqlNodeInterpreterFactory>
        NodeInterpretersCreator =>
        _nodeInterpretersCreator ?? BaseNodeInterpretersCreator;

    /// <summary>
    /// Creates a new <see cref="PostgreSqlDatabaseFactoryOptions"/> instance
    /// with changed <see cref="VirtualGeneratedColumnStorageResolution"/>.
    /// </summary>
    /// <param name="resolution">Value to set.</param>
    /// <returns>New <see cref="PostgreSqlDatabaseFactoryOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="PostgreSqlDatabaseFactoryOptions"/> instance with changed <see cref="EncodingName"/>.
    /// </summary>
    /// <param name="name">Value to set.</param>
    /// <returns>New <see cref="PostgreSqlDatabaseFactoryOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="PostgreSqlDatabaseFactoryOptions"/> instance with changed <see cref="LocaleName"/>.
    /// </summary>
    /// <param name="name">Value to set.</param>
    /// <returns>New <see cref="PostgreSqlDatabaseFactoryOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="PostgreSqlDatabaseFactoryOptions"/> instance with changed <see cref="ConcurrentConnectionsLimit"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns>New <see cref="PostgreSqlDatabaseFactoryOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="PostgreSqlDatabaseFactoryOptions"/> instance with changed <see cref="DefaultNamesCreator"/>.
    /// </summary>
    /// <param name="creator">Value to set.</param>
    /// <returns>New <see cref="PostgreSqlDatabaseFactoryOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="PostgreSqlDatabaseFactoryOptions"/> instance with changed <see cref="TypeDefinitionsCreator"/>.
    /// </summary>
    /// <param name="creator">Value to set.</param>
    /// <returns>New <see cref="PostgreSqlDatabaseFactoryOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="PostgreSqlDatabaseFactoryOptions"/> instance with changed <see cref="NodeInterpretersCreator"/>.
    /// </summary>
    /// <param name="creator">Value to set.</param>
    /// <returns>New <see cref="PostgreSqlDatabaseFactoryOptions"/> instance.</returns>
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
