using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql;

public readonly struct MySqlDatabaseFactoryOptions
{
    public static readonly MySqlDatabaseFactoryOptions Default = new MySqlDatabaseFactoryOptions();

    public static readonly SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider>
        BaseTypeDefinitionsCreator = static (_, _) => new MySqlColumnTypeDefinitionProviderBuilder().Build();

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

    public SqlOptionalFunctionalityResolution IndexFilterResolution { get; }
    public string? CharacterSetName { get; }
    public string? CollationName { get; }
    public bool? IsEncryptionEnabled { get; }

    public SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider> DefaultNamesCreator =>
        _defaultNamesCreator ?? SqlHelpers.DefaultNamesCreator;

    public SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider> TypeDefinitionsCreator =>
        _typeDefinitionsCreator ?? BaseTypeDefinitionsCreator;

    public SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider, MySqlNodeInterpreterFactory>
        NodeInterpretersCreator =>
        _nodeInterpretersCreator ?? BaseNodeInterpretersCreator;

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
