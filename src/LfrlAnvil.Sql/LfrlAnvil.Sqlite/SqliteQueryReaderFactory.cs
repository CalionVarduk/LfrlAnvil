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

using LfrlAnvil.Sql.Statements.Compilers;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

/// <summary>
/// Represents a factory of delegates used by query reader expression instances.
/// </summary>
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteQueryReaderFactory : SqlQueryReaderFactory<SqliteDataReader>
{
    internal SqliteQueryReaderFactory(SqliteColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( SqliteDialect.Instance, columnTypeDefinitions ) { }
}
