using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Tests.Helpers;

internal sealed class MySqlDatabaseBuilderMock
{
    [Pure]
    internal static MySqlDatabaseBuilder Create(
        SqlOptionalFunctionalityResolution indexFilterResolution = SqlOptionalFunctionalityResolution.Ignore,
        params SqlColumnTypeDefinition[] typeDefinitions)
    {
        var typeBuilder = new MySqlColumnTypeDefinitionProviderBuilder();
        foreach ( var definition in typeDefinitions )
            typeBuilder.Register( definition );

        var result = new MySqlDatabaseBuilder(
            "0.0.0",
            MySqlHelpers.DefaultVersionHistoryName.Schema,
            new SqlDefaultObjectNameProvider(),
            new MySqlDataTypeProvider(),
            new MySqlColumnTypeDefinitionProvider( typeBuilder ),
            new MySqlNodeInterpreterFactory( MySqlNodeInterpreterOptions.Default.EnableIndexFilterParsing() ),
            indexFilterResolution );

        result.Changes.SetModeAndAttach( SqlDatabaseCreateMode.DryRun );
        return result;
    }
}
