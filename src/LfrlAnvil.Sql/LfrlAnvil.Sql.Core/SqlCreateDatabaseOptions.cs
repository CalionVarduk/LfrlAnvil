using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public readonly record struct SqlCreateDatabaseOptions
{
    public static readonly SqlCreateDatabaseOptions Default = new SqlCreateDatabaseOptions();

    public readonly SqlDatabaseCreateMode Mode;
    public readonly string? VersionHistorySchemaName;
    public readonly string? VersionHistoryTableName;
    public readonly SqlDatabaseVersionHistoryPersistenceMode VersionHistoryPersistenceMode;

    private SqlCreateDatabaseOptions(
        SqlDatabaseCreateMode mode,
        string? versionHistorySchemaName,
        string? versionHistoryTableName,
        SqlDatabaseVersionHistoryPersistenceMode versionHistoryPersistenceMode)
    {
        Mode = mode;
        VersionHistorySchemaName = versionHistorySchemaName;
        VersionHistoryTableName = versionHistoryTableName;
        VersionHistoryPersistenceMode = versionHistoryPersistenceMode;
    }

    [Pure]
    public SqlCreateDatabaseOptions SetMode(SqlDatabaseCreateMode mode)
    {
        Ensure.IsDefined( mode, nameof( mode ) );
        return new SqlCreateDatabaseOptions( mode, VersionHistorySchemaName, VersionHistoryTableName, VersionHistoryPersistenceMode );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistorySchemaName(string? name)
    {
        return new SqlCreateDatabaseOptions( Mode, name, VersionHistoryTableName, VersionHistoryPersistenceMode );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistoryTableName(string? name)
    {
        return new SqlCreateDatabaseOptions( Mode, VersionHistorySchemaName, name, VersionHistoryPersistenceMode );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistoryPersistenceMode(SqlDatabaseVersionHistoryPersistenceMode mode)
    {
        Ensure.IsDefined( mode, nameof( mode ) );
        return new SqlCreateDatabaseOptions( Mode, VersionHistorySchemaName, VersionHistoryTableName, mode );
    }
}
