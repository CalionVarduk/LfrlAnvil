using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.PostgreSql.Versioning;

public static class PostgreSqlDatabaseVersion
{
    [Pure]
    public static SqlDatabaseVersion<PostgreSqlDatabaseBuilder> Create(
        Version value,
        string? description,
        Action<PostgreSqlDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, description, apply );
    }

    [Pure]
    public static SqlDatabaseVersion<PostgreSqlDatabaseBuilder> Create(Version value, Action<PostgreSqlDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, null, apply );
    }
}
