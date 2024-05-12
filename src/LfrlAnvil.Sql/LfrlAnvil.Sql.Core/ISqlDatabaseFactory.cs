using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a factory of SQL databases.
/// </summary>
public interface ISqlDatabaseFactory
{
    /// <summary>
    /// Specifies the SQL dialect of this factory.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Attempts to create a new <see cref="ISqlDatabase"/> instance from the provided history of versions.
    /// </summary>
    /// <param name="connectionString">Connection string to the database.</param>
    /// <param name="versionHistory">Collection of DB versions.</param>
    /// <param name="options">DB creation options.</param>
    /// <returns>New <see cref="SqlCreateDatabaseResult{TDatabase}"/> instance.</returns>
    SqlCreateDatabaseResult<ISqlDatabase> Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options = default);
}
