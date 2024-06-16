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

using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a factory of SQL databases.
/// </summary>
public interface ISqlDatabaseFactory
{
    /// <summary>
    /// Specifies the SQL dialect of this factory.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Attempts to create a new <see cref="ISqlDatabase"/> instance from the provided history of versions.
    /// </summary>
    /// <param name="connectionString">Connection string to the database.</param>
    /// <param name="versionHistory">Collection of DB versions.</param>
    /// <param name="options">DB creation options.</param>
    /// <returns>New <see cref="SqlCreateDatabaseResult{TDatabase}"/> instance.</returns>
    SqlCreateDatabaseResult<ISqlDatabase> Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options = default);
}
