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

using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node that defines a primary key constraint.
/// </summary>
public sealed class SqlPrimaryKeyDefinitionNode : SqlNodeBase
{
    internal SqlPrimaryKeyDefinitionNode(SqlSchemaObjectName name, ReadOnlyArray<SqlOrderByNode> columns)
        : base( SqlNodeType.PrimaryKeyDefinition )
    {
        Name = name;
        Columns = columns;
    }

    /// <summary>
    /// Primary key constraint's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <summary>
    /// Collection of columns that define this primary key constraint.
    /// </summary>
    public ReadOnlyArray<SqlOrderByNode> Columns { get; }
}
