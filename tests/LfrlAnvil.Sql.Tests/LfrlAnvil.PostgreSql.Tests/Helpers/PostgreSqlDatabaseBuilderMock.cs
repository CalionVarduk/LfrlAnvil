using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Internal;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Tests.Helpers;

internal sealed class PostgreSqlDatabaseBuilderMock
{
    [Pure]
    internal static PostgreSqlDatabaseBuilder Create(
        SqlOptionalFunctionalityResolution virtualGeneratedColumnStorageResolution = SqlOptionalFunctionalityResolution.Ignore,
        params SqlColumnTypeDefinition[] typeDefinitions)
    {
        var typeBuilder = new PostgreSqlColumnTypeDefinitionProviderBuilder();
        foreach ( var definition in typeDefinitions )
            typeBuilder.Register( definition );

        var result = new PostgreSqlDatabaseBuilder(
            "0.0.0",
            PostgreSqlHelpers.DefaultVersionHistoryName.Schema,
            new SqlDefaultObjectNameProvider(),
            new PostgreSqlDataTypeProvider(),
            new PostgreSqlColumnTypeDefinitionProvider( typeBuilder ),
            new PostgreSqlNodeInterpreterFactory( PostgreSqlNodeInterpreterOptions.Default.EnableVirtualGeneratedColumnStorageParsing() ),
            virtualGeneratedColumnStorageResolution );

        result.Changes.SetModeAndAttach( SqlDatabaseCreateMode.DryRun );
        return result;
    }
}
