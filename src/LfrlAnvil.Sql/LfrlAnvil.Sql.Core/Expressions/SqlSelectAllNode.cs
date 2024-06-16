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

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree select node that defines a query selection of all <see cref="DataSource"/> fields.
/// </summary>
public sealed class SqlSelectAllNode : SqlSelectNode
{
    internal SqlSelectAllNode(SqlDataSourceNode dataSource)
        : base( SqlNodeType.SelectAll )
    {
        DataSource = dataSource;
    }

    /// <summary>
    /// Data source to select all data fields from.
    /// </summary>
    public SqlDataSourceNode DataSource { get; }

    internal override void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor)
    {
        foreach ( var recordSet in DataSource.RecordSets )
        {
            foreach ( var field in recordSet.GetKnownFields() )
                visitor.Handle( field.Name, field );
        }
    }
}
