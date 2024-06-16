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

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single common table expression (CTE).
/// </summary>
public abstract class SqlCommonTableExpressionNode : SqlNodeBase
{
    internal SqlCommonTableExpressionNode(SqlQueryExpressionNode query, string name, bool isRecursive)
        : base( SqlNodeType.CommonTableExpression )
    {
        Query = query;
        Name = name;
        IsRecursive = isRecursive;
        RecordSet = new SqlCommonTableExpressionRecordSetNode( this, alias: null, isOptional: false );
    }

    /// <summary>
    /// Underlying query that defines this common table expression.
    /// </summary>
    public SqlQueryExpressionNode Query { get; }

    /// <summary>
    /// Name of this common table expression.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Specifies whether or not this common table expression is recursive.
    /// </summary>
    public bool IsRecursive { get; }

    /// <summary>
    /// <see cref="SqlCommonTableExpressionRecordSetNode"/> instance associated with this common table expression.
    /// </summary>
    public SqlCommonTableExpressionRecordSetNode RecordSet { get; }
}
