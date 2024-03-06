﻿using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;

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
        Apply( SqlHelpers.CastOrThrow<SqliteDatabaseBuilder>( SqliteDialect.Instance, database ) );
    }

    protected abstract void Apply(SqliteDatabaseBuilder database);
}
