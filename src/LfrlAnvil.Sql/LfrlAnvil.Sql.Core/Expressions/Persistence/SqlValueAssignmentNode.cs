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

using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

/// <summary>
/// Represents an SQL syntax tree node that defines a value assignment.
/// </summary>
public sealed class SqlValueAssignmentNode : SqlNodeBase
{
    internal SqlValueAssignmentNode(SqlDataFieldNode dataField, SqlExpressionNode value)
        : base( SqlNodeType.ValueAssignment )
    {
        DataField = dataField;
        Value = value;
    }

    /// <summary>
    /// Data field to assign <see cref="Value"/> to.
    /// </summary>
    public SqlDataFieldNode DataField { get; }

    /// <summary>
    /// Value to assign.
    /// </summary>
    public SqlExpressionNode Value { get; }
}
