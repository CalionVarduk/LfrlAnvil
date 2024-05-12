using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Tests.Helpers;

internal static class SqliteDatabaseBuilderMock
{
    [Pure]
    internal static SqliteDatabaseBuilder Create(bool arePositionalParametersEnabled = false)
    {
        var typeDefinitions = new SqliteColumnTypeDefinitionProviderBuilder().Build();
        var result = new SqliteDatabaseBuilder(
            "0.0.0",
            SqliteHelpers.DefaultVersionHistoryName.Schema,
            new SqlDefaultObjectNameProvider(),
            new SqliteDataTypeProvider(),
            typeDefinitions,
            new SqliteNodeInterpreterFactory(
                SqliteNodeInterpreterOptions.Default.EnablePositionalParameters( arePositionalParametersEnabled ) ) );

        result.Changes.SetModeAndAttach( SqlDatabaseCreateMode.DryRun );
        return result;
    }
}
