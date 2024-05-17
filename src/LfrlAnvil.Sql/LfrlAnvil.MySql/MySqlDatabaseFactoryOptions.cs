using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql;

/// <summary>
/// Represents available options for creating MySQL database objects through <see cref="MySqlDatabaseFactory"/>.
/// </summary>
public readonly struct MySqlDatabaseFactoryOptions
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static readonly MySqlDatabaseFactoryOptions Default = new MySqlDatabaseFactoryOptions();

    /// <summary>
    /// Default creator of <see cref="MySqlColumnTypeDefinitionProvider"/> instances.
    /// </summary>
    public static readonly SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider>
        BaseTypeDefinitionsCreator = static (_, _) => new MySqlColumnTypeDefinitionProviderBuilder().Build();

    /// <summary>
    /// Default creator of <see cref="MySqlNodeInterpreterFactory"/> instances.
    /// </summary>
    public static readonly
        SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider, MySqlNodeInterpreterFactory>
        BaseNodeInterpretersCreator = static (_, defaultSchemaName, _, typeDefinitions) =>
            new MySqlNodeInterpreterFactory(
                MySqlNodeInterpreterOptions.Default.SetCommonSchemaName( defaultSchemaName ).SetTypeDefinitions( typeDefinitions ) );

    private readonly SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? _defaultNamesCreator;

    private readonly SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider>?
        _typeDefinitionsCreator;

    private readonly
        SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider, MySqlNodeInterpreterFactory>?
        _nodeInterpretersCreator;

    private MySqlDatabaseFactoryOptions(
        SqlOptionalFunctionalityResolution indexFilterResolution,
        string? characterSetName,
        string? collationName,
        bool? isEncryptionEnabled,
        SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? defaultNamesCreator,
        SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider>? typeDefinitionsCreator,
        SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider, MySqlNodeInterpreterFactory>?
            nodeInterpretersCreator)
    {
        IndexFilterResolution = indexFilterResolution;
        CharacterSetName = characterSetName;
        CollationName = collationName;
        IsEncryptionEnabled = isEncryptionEnabled;
        _defaultNamesCreator = defaultNamesCreator;
        _typeDefinitionsCreator = typeDefinitionsCreator;
        _nodeInterpretersCreator = nodeInterpretersCreator;
    }

    /// <summary>
    /// Specifies how partial indexes should be resolved by DB factory.
    /// </summary>
    public SqlOptionalFunctionalityResolution IndexFilterResolution { get; }

    /// <summary>
    /// Specifies default DB character set.
    /// </summary>
    public string? CharacterSetName { get; }

    /// <summary>
    /// Specifies default DB collation.
    /// </summary>
    public string? CollationName { get; }

    /// <summary>
    /// Specifies whether or not DB encryption should be enabled.
    /// </summary>
    public bool? IsEncryptionEnabled { get; }

    /// <summary>
    /// Specifies the creator of <see cref="SqlDefaultObjectNameProvider"/> instances.
    /// </summary>
    public SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider> DefaultNamesCreator =>
        _defaultNamesCreator ?? SqlHelpers.DefaultNamesCreator;

    /// <summary>
    /// Specifies the creator of <see cref="MySqlColumnTypeDefinitionProvider"/> instances.
    /// </summary>
    public SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider> TypeDefinitionsCreator =>
        _typeDefinitionsCreator ?? BaseTypeDefinitionsCreator;

    /// <summary>
    /// Specifies the creator of <see cref="MySqlNodeInterpreterFactory"/> instances.
    /// </summary>
    public SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider, MySqlNodeInterpreterFactory>
        NodeInterpretersCreator =>
        _nodeInterpretersCreator ?? BaseNodeInterpretersCreator;

    /// <summary>
    /// Creates a new <see cref="MySqlDatabaseFactoryOptions"/> instance with changed <see cref="IndexFilterResolution"/>.
    /// </summary>
    /// <param name="resolution">Value to set.</param>
    /// <returns>New <see cref="MySqlDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions SetIndexFilterResolution(SqlOptionalFunctionalityResolution resolution)
    {
        Ensure.IsDefined( resolution );
        return new MySqlDatabaseFactoryOptions(
            resolution,
            CharacterSetName,
            CollationName,
            IsEncryptionEnabled,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlDatabaseFactoryOptions"/> instance with changed <see cref="CharacterSetName"/>.
    /// </summary>
    /// <param name="name">Value to set.</param>
    /// <returns>New <see cref="MySqlDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions SetCharacterSetName(string? name)
    {
        return new MySqlDatabaseFactoryOptions(
            IndexFilterResolution,
            name,
            CollationName,
            IsEncryptionEnabled,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlDatabaseFactoryOptions"/> instance with changed <see cref="CollationName"/>.
    /// </summary>
    /// <param name="name">Value to set.</param>
    /// <returns>New <see cref="MySqlDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions SetCollationName(string? name)
    {
        return new MySqlDatabaseFactoryOptions(
            IndexFilterResolution,
            CharacterSetName,
            name,
            IsEncryptionEnabled,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlDatabaseFactoryOptions"/> instance with changed <see cref="IsEncryptionEnabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="MySqlDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions EnableEncryption(bool? enabled = true)
    {
        return new MySqlDatabaseFactoryOptions(
            IndexFilterResolution,
            CharacterSetName,
            CollationName,
            enabled,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlDatabaseFactoryOptions"/> instance with changed <see cref="DefaultNamesCreator"/>.
    /// </summary>
    /// <param name="creator">Value to set.</param>
    /// <returns>New <see cref="MySqlDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions SetDefaultNamesCreator(SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? creator)
    {
        return new MySqlDatabaseFactoryOptions(
            IndexFilterResolution,
            CharacterSetName,
            CollationName,
            IsEncryptionEnabled,
            creator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlDatabaseFactoryOptions"/> instance with changed <see cref="TypeDefinitionsCreator"/>.
    /// </summary>
    /// <param name="creator">Value to set.</param>
    /// <returns>New <see cref="MySqlDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions SetTypeDefinitionsCreator(
        SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider>? creator)
    {
        return new MySqlDatabaseFactoryOptions(
            IndexFilterResolution,
            CharacterSetName,
            CollationName,
            IsEncryptionEnabled,
            _defaultNamesCreator,
            creator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="MySqlDatabaseFactoryOptions"/> instance with changed <see cref="NodeInterpretersCreator"/>.
    /// </summary>
    /// <param name="creator">Value to set.</param>
    /// <returns>New <see cref="MySqlDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions SetNodeInterpretersCreator(
        SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider, MySqlNodeInterpreterFactory>? creator)
    {
        return new MySqlDatabaseFactoryOptions(
            IndexFilterResolution,
            CharacterSetName,
            CollationName,
            IsEncryptionEnabled,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            creator );
    }
}
