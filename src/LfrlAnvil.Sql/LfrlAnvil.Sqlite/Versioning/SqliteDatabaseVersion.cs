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
