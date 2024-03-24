using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public interface ISqliteDatabaseConnector : ISqlDatabaseConnector<SqliteConnection>
{
    new SqliteDatabase Database { get; }
}
