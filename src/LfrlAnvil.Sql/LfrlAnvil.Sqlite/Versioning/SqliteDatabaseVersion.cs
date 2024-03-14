using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Versioning;

public static class SqliteDatabaseVersion
{
    [Pure]
    public static SqlDatabaseVersion<SqliteDatabaseBuilder> Create(Version value, string? description, Action<SqliteDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, description, apply );
    }

    [Pure]
    public static SqlDatabaseVersion<SqliteDatabaseBuilder> Create(Version value, Action<SqliteDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, null, apply );
    }
}
