using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Versioning;

public abstract class SqlDatabaseVersion
{
    protected SqlDatabaseVersion(Version value, string? description = null)
    {
        Value = value;
        Description = description ?? string.Empty;
    }

    public Version Value { get; }
    public string Description { get; }

    [Pure]
    public static SqlDatabaseVersion Create(Version value, string? description, Action<ISqlDatabaseBuilder> apply)
    {
        return new SqlDatabaseLambdaVersion( value, description, apply );
    }

    [Pure]
    public static SqlDatabaseVersion Create(Version value, Action<ISqlDatabaseBuilder> apply)
    {
        return Create( value, null, apply );
    }

    [Pure]
    public override string ToString()
    {
        return Description.Length > 0 ? $"{Value} ({Description})" : Value.ToString();
    }

    public abstract void Apply(ISqlDatabaseBuilder database);
}
