using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Versioning;

public abstract class SqliteDatabaseVersion : SqlDatabaseVersion
{
    protected SqliteDatabaseVersion(Version value, string? description = null)
        : base( value, description ) { }

    [Pure]
    public static SqliteDatabaseVersion Create(Version value, string? description, Action<SqliteDatabaseBuilder> apply)
    {
        return new SqliteDatabaseLambdaVersion( value, description ?? string.Empty, apply );
    }

    [Pure]
    public static SqliteDatabaseVersion Create(Version value, Action<SqliteDatabaseBuilder> apply)
    {
        return Create( value, null, apply );
    }

    public sealed override void Apply(ISqlDatabaseBuilder database)
    {
        Apply( SqliteHelpers.CastOrThrow<SqliteDatabaseBuilder>( database ) );
    }

    protected abstract void Apply(SqliteDatabaseBuilder database);
}
