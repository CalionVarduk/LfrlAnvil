using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Versioning;

/// <summary>
/// Represents a single database version.
/// </summary>
/// <typeparam name="TDatabaseBuilder">SQL database builder type.</typeparam>
public abstract class SqlDatabaseVersion<TDatabaseBuilder> : ISqlDatabaseVersion
    where TDatabaseBuilder : class, ISqlDatabaseBuilder
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="description">Optional description of this version.</param>
    protected SqlDatabaseVersion(Version value, string? description = null)
    {
        Value = value;
        Description = description ?? string.Empty;
    }

    /// <inheritdoc />
    public Version Value { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc cref="ISqlDatabaseVersion.Apply(ISqlDatabaseBuilder)" />
    public abstract void Apply(TDatabaseBuilder database);

    /// <summary>
    /// Returns a string representation of this <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
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

/// <summary>
/// Creates instances of <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> type.
/// </summary>
public static class SqlDatabaseVersion
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="description">Optional description of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <typeparam name="TDatabaseBuilder">SQL database builder type.</typeparam>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<TDatabaseBuilder> Create<TDatabaseBuilder>(
        Version value,
        string? description,
        Action<TDatabaseBuilder> apply)
        where TDatabaseBuilder : class, ISqlDatabaseBuilder
    {
        return new SqlLambdaDatabaseVersion<TDatabaseBuilder>( apply, value, description );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <typeparam name="TDatabaseBuilder">SQL database builder type.</typeparam>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<TDatabaseBuilder> Create<TDatabaseBuilder>(Version value, Action<TDatabaseBuilder> apply)
        where TDatabaseBuilder : class, ISqlDatabaseBuilder
    {
        return Create( value, null, apply );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="description">Optional description of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<SqlDatabaseBuilder> Create(Version value, string? description, Action<SqlDatabaseBuilder> apply)
    {
        return Create<SqlDatabaseBuilder>( value, description, apply );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<SqlDatabaseBuilder> Create(Version value, Action<SqlDatabaseBuilder> apply)
    {
        return Create<SqlDatabaseBuilder>( value, apply );
    }
}
