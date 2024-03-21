using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

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
        SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? defaultNamesCreator,
        SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider>? typeDefinitionsCreator,
        SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider, MySqlNodeInterpreterFactory>?
            nodeInterpretersCreator)
    {
        IndexFilterResolution = indexFilterResolution;
        _defaultNamesCreator = defaultNamesCreator;
        _typeDefinitionsCreator = typeDefinitionsCreator;
        _nodeInterpretersCreator = nodeInterpretersCreator;
    }

    public SqlOptionalFunctionalityResolution IndexFilterResolution { get; }

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
        return new MySqlDatabaseFactoryOptions( resolution, _defaultNamesCreator, _typeDefinitionsCreator, _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions SetDefaultNamesCreator(SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? creator)
    {
        return new MySqlDatabaseFactoryOptions( IndexFilterResolution, creator, _typeDefinitionsCreator, _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions SetTypeDefinitionsCreator(
        SqlColumnTypeDefinitionProviderCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider>? creator)
    {
        return new MySqlDatabaseFactoryOptions( IndexFilterResolution, _defaultNamesCreator, creator, _nodeInterpretersCreator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlDatabaseFactoryOptions SetNodeInterpretersCreator(
        SqlNodeInterpreterFactoryCreator<MySqlDataTypeProvider, MySqlColumnTypeDefinitionProvider, MySqlNodeInterpreterFactory>? creator)
    {
        return new MySqlDatabaseFactoryOptions( IndexFilterResolution, _defaultNamesCreator, _typeDefinitionsCreator, creator );
    }
}
