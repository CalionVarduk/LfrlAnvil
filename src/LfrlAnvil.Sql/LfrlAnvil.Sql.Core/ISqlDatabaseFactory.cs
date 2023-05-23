using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

public interface ISqlDatabaseFactory
{
    SqlDialect Dialect { get; }

    SqlCreateDatabaseResult<ISqlDatabase> Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options = default);
}
