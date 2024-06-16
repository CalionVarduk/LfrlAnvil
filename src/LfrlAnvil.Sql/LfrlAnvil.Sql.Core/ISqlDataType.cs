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
using System.Data;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a DB data type.
/// </summary>
public interface ISqlDataType
{
    /// <summary>
    /// Specifies the SQL dialect of this data type.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// DB name of this data type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// <see cref="System.Data.DbType"/> of this data type.
    /// </summary>
    DbType DbType { get; }

    /// <summary>
    /// Collection of applied parameters to this data type.
    /// </summary>
    ReadOnlySpan<int> Parameters { get; }

    /// <summary>
    /// Collection of parameter definitions for this data type.
    /// </summary>
    ReadOnlySpan<SqlDataTypeParameter> ParameterDefinitions { get; }
}
