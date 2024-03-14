using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Versioning;

public abstract class SqlDatabaseVersion<TDatabaseBuilder> : ISqlDatabaseVersion
    where TDatabaseBuilder : class, ISqlDatabaseBuilder
{
    protected SqlDatabaseVersion(Version value, string? description = null)
    {
        Value = value;
        Description = description ?? string.Empty;
    }

    public Version Value { get; }
    public string Description { get; }

    public abstract void Apply(TDatabaseBuilder database);

    [Pure]
    public override string ToString()
    {
        return Description.Length > 0 ? $"{Value} ({Description})" : Value.ToString();
    }

    void ISqlDatabaseVersion.Apply(ISqlDatabaseBuilder database)
    {
        Apply( SqlHelpers.CastOrThrow<TDatabaseBuilder>( database, database ) );
    }
}

public static class SqlDatabaseVersion
{
    [Pure]
    public static SqlDatabaseVersion<TDatabaseBuilder> Create<TDatabaseBuilder>(
        Version value,
        string? description,
        Action<TDatabaseBuilder> apply)
        where TDatabaseBuilder : class, ISqlDatabaseBuilder
    {
        return new SqlLambdaDatabaseVersion<TDatabaseBuilder>( apply, value, description );
    }

    [Pure]
    public static SqlDatabaseVersion<TDatabaseBuilder> Create<TDatabaseBuilder>(Version value, Action<TDatabaseBuilder> apply)
        where TDatabaseBuilder : class, ISqlDatabaseBuilder
    {
        return Create( value, null, apply );
    }

    [Pure]
    public static SqlDatabaseVersion<SqlDatabaseBuilder> Create(Version value, string? description, Action<SqlDatabaseBuilder> apply)
    {
        return Create<SqlDatabaseBuilder>( value, description, apply );
    }

    [Pure]
    public static SqlDatabaseVersion<SqlDatabaseBuilder> Create(Version value, Action<SqlDatabaseBuilder> apply)
    {
        return Create<SqlDatabaseBuilder>( value, apply );
    }
}
