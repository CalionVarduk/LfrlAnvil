using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Tests.Helpers;

internal sealed class MySqlDatabaseBuilderMock
{
    [Pure]
    internal static MySqlDatabaseBuilder Create()
    {
        var result = new MySqlDatabaseBuilder( "0.0.0" );
        result.ChangeTracker.ClearStatements();
        return result;
    }
}
