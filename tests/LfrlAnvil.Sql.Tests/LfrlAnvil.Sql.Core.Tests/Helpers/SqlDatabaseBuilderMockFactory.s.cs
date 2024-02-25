using System.Diagnostics.Contracts;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class SqlDatabaseBuilderMockFactory
{
    [Pure]
    public static SqlDatabaseBuilderMock Create(
        string serverVersion = "0.0.0",
        string defaultSchemaName = "common")
    {
        var result = SqlDatabaseBuilderMock.Create( serverVersion, defaultSchemaName );
        result.Changes.SetModeAndAttach( SqlDatabaseCreateMode.DryRun );
        return result;
    }
}
