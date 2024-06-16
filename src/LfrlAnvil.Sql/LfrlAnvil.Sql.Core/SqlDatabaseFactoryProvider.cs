// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
