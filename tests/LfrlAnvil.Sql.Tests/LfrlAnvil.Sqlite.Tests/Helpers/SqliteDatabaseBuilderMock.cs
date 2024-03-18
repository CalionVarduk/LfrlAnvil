using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Tests.Helpers;

internal sealed class SqliteDatabaseBuilderMock
{
    [Pure]
    internal static SqliteDatabaseBuilder Create()
    {
        var typeDefinitions = new SqliteColumnTypeDefinitionProviderBuilder().Build();
        var result = new SqliteDatabaseBuilder(
            "0.0.0",
            SqliteHelpers.DefaultVersionHistoryName.Schema,
            new SqlDefaultObjectNameProvider(),
            new SqliteDataTypeProvider(),
            typeDefinitions,
            new SqliteNodeInterpreterFactory( SqliteNodeInterpreterOptions.Default ) );

        result.Changes.SetModeAndAttach( SqlDatabaseCreateMode.DryRun );
        return result;
    }
}
