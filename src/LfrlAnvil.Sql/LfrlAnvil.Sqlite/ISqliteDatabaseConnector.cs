using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc cref="ISqlDatabaseConnector" />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public interface ISqliteDatabaseConnector : ISqlDatabaseConnector<SqliteConnection>
{
    /// <inheritdoc cref="ISqlDatabaseConnector{TConnection}.Database" />
    new SqliteDatabase Database { get; }
}
