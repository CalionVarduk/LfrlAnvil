using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Events;

/// <summary>
/// Represents an identifier of an SQL statement ran during <see cref="ISqlDatabase"/> creation.
/// </summary>
/// <param name="Version">Version associated with the SQL statement.</param>
/// <param name="Ordinal">Ordinal of the SQL statement.</param>
public readonly record struct SqlDatabaseFactoryStatementKey(Version Version, int Ordinal)
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseFactoryStatementKey"/> instance with <see cref="Ordinal"/> equal to <b>0</b>.
    /// </summary>
    /// <param name="version">Version associated with the SQL statement.</param>
    /// <returns>New <see cref="SqlDatabaseFactoryStatementKey"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseFactoryStatementKey Create(Version version)
    {
        return new SqlDatabaseFactoryStatementKey( version, Ordinal: 0 );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseFactoryStatementKey"/> instance with <see cref="Ordinal"/> incremented by <b>1</b>.
    /// </summary>
    /// <returns>New <see cref="SqlDatabaseFactoryStatementKey"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlDatabaseFactoryStatementKey NextOrdinal()
    {
        return new SqlDatabaseFactoryStatementKey( Version, Ordinal + 1 );
    }
}
