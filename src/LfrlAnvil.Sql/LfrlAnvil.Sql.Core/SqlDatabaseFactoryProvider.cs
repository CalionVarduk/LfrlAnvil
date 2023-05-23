using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public sealed class SqlDatabaseFactoryProvider
{
    private readonly Dictionary<SqlDialect, ISqlDatabaseFactory> _databaseProviders;

    public SqlDatabaseFactoryProvider()
    {
        _databaseProviders = new Dictionary<SqlDialect, ISqlDatabaseFactory>();
    }

    public IReadOnlyCollection<SqlDialect> SupportedDialects => _databaseProviders.Keys;

    [Pure]
    public ISqlDatabaseFactory GetFor(SqlDialect dialect)
    {
        return _databaseProviders[dialect];
    }

    public SqlDatabaseFactoryProvider RegisterFactory(ISqlDatabaseFactory factory)
    {
        _databaseProviders[factory.Dialect] = factory;
        return this;
    }
}
