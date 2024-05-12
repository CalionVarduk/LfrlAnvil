using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a collection of <see cref="ISqlDatabaseFactory"/> instances identifiable by their <see cref="ISqlDatabaseFactory.Dialect"/>.
/// </summary>
public sealed class SqlDatabaseFactoryProvider
{
    private readonly Dictionary<SqlDialect, ISqlDatabaseFactory> _databaseProviders;

    /// <summary>
    /// Creates a new empty <see cref="SqlDatabaseFactoryProvider"/> instance.
    /// </summary>
    public SqlDatabaseFactoryProvider()
    {
        _databaseProviders = new Dictionary<SqlDialect, ISqlDatabaseFactory>();
    }

    /// <summary>
    /// Collection of registered SQL dialects.
    /// </summary>
    public IReadOnlyCollection<SqlDialect> SupportedDialects => _databaseProviders.Keys;

    /// <summary>
    /// Returns an <see cref="ISqlDatabaseFactory"/> instance associated with the provided <paramref name="dialect"/>.
    /// </summary>
    /// <param name="dialect">SQL dialect.</param>
    /// <returns><see cref="ISqlDatabaseFactory"/> instance associated with the provided <paramref name="dialect"/>.</returns>
    /// <exception cref="KeyNotFoundException">When <paramref name="dialect"/> was not registered.</exception>
    [Pure]
    public ISqlDatabaseFactory GetFor(SqlDialect dialect)
    {
        return _databaseProviders[dialect];
    }

    /// <summary>
    /// Registers the provided <paramref name="factory"/>.
    /// </summary>
    /// <param name="factory">Factory to register.</param>
    /// <returns><b>this</b>.</returns>
    public SqlDatabaseFactoryProvider RegisterFactory(ISqlDatabaseFactory factory)
    {
        _databaseProviders[factory.Dialect] = factory;
        return this;
    }
}
