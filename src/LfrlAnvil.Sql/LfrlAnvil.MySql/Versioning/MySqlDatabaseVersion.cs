using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.MySql.Versioning;

public static class MySqlDatabaseVersion
{
    [Pure]
    public static SqlDatabaseVersion<MySqlDatabaseBuilder> Create(Version value, string? description, Action<MySqlDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, description, apply );
    }

    [Pure]
    public static SqlDatabaseVersion<MySqlDatabaseBuilder> Create(Version value, Action<MySqlDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, null, apply );
    }
}
