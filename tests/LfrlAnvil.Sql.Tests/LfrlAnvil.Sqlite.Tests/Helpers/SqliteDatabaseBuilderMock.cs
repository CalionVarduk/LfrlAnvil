using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Tests.Helpers;

internal sealed class SqliteDatabaseBuilderMock
{
    [Pure]
    internal static SqliteDatabaseBuilder Create()
    {
        var result = new SqliteDatabaseBuilder(
            "0.0.0",
            SqliteHelpers.DefaultVersionHistoryName.Schema,
            new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() ) );

        result.Changes.SetModeAndAttach( SqlDatabaseCreateMode.DryRun );
        return result;
    }
}
