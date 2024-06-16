﻿// Copyright 2024 Łukasz Furlepa
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

using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite.Extensions;

/// <summary>
/// Contains <see cref="SqlDatabaseFactoryProvider"/> extension methods.
/// </summary>
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public static class SqlDatabaseFactoryProviderExtensions
{
    /// <summary>
    /// Registers a new <see cref="SqliteDatabaseFactory"/> instance in the <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Source provider.</param>
    /// <param name="options">
    /// Optional <see cref="SqliteDatabaseFactoryOptions"/>. Equal to <see cref="SqliteDatabaseFactoryOptions.Default"/> by default.
    /// </param>
    /// <returns><paramref name="provider"/>.</returns>
    public static SqlDatabaseFactoryProvider RegisterSqlite(
        this SqlDatabaseFactoryProvider provider,
        SqliteDatabaseFactoryOptions? options = null)
    {
        return provider.RegisterFactory( new SqliteDatabaseFactory( options ) );
    }
}
