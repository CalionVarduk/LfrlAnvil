using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.PostgreSql.Versioning;

/// <summary>
/// Creates instances of <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> type for <see cref="PostgreSqlDatabaseBuilder"/>.
/// </summary>
public static class PostgreSqlDatabaseVersion
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance for <see cref="PostgreSqlDatabaseBuilder"/>.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="description">Optional description of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<PostgreSqlDatabaseBuilder> Create(
        Version value,
        string? description,
        Action<PostgreSqlDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, description, apply );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance for <see cref="PostgreSqlDatabaseBuilder"/>.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<PostgreSqlDatabaseBuilder> Create(Version value, Action<PostgreSqlDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, null, apply );
    }
}
