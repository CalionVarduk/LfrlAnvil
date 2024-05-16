using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Versioning;

/// <summary>
/// Creates instances of <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> type for <see cref="SqliteDatabaseBuilder"/>.
/// </summary>
public static class SqliteDatabaseVersion
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance for <see cref="SqliteDatabaseBuilder"/>.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="description">Optional description of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<SqliteDatabaseBuilder> Create(Version value, string? description, Action<SqliteDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, description, apply );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance for <see cref="SqliteDatabaseBuilder"/>.
    /// </summary>
    /// <param name="value">Identifier of this version.</param>
    /// <param name="apply">Delegate that defines this version's changes.</param>
    /// <returns>New <see cref="SqlDatabaseVersion{TDatabaseBuilder}"/> instance.</returns>
    [Pure]
    public static SqlDatabaseVersion<SqliteDatabaseBuilder> Create(Version value, Action<SqliteDatabaseBuilder> apply)
    {
        return SqlDatabaseVersion.Create( value, null, apply );
    }
}
