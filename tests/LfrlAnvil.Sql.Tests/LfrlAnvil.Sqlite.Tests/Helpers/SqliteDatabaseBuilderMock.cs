using System.Diagnostics.Contracts;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Tests.Helpers;

internal sealed class SqliteDatabaseBuilderMock
{
    [Pure]
    internal static SqliteDatabaseBuilder Create()
    {
        return new SqliteDatabaseBuilder( "0.0.0" );
    }
}
