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
using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

/// <summary>
/// Represents an SQL syntax tree statement node that defines an insertion of new records to a table
/// or update of existing records in that table.
/// </summary>
public sealed class SqlUpsertNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlUpsertNode(
        SqlQueryExpressionNode query,
        SqlRecordSetNode recordSet,
        ReadOnlyArray<SqlDataFieldNode> insertDataFields,
        ReadOnlyArray<SqlDataFieldNode> conflictTarget,
        Func<SqlRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>> updateAssignments)
        : base( SqlNodeType.Upsert )
    {
        Source = query;
        RecordSet = recordSet;
        InsertDataFields = insertDataFields;
        ConflictTarget = conflictTarget;
        UpdateSource = new SqlInternalRecordSetNode( recordSet );
        UpdateAssignments = updateAssignments( RecordSet, UpdateSource ).ToArray();
    }

    internal SqlUpsertNode(
        SqlValuesNode values,
        SqlRecordSetNode recordSet,
        ReadOnlyArray<SqlDataFieldNode> insertDataFields,
        ReadOnlyArray<SqlDataFieldNode> conflictTarget,
        Func<SqlRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>> updateAssignments)
        : base( SqlNodeType.Upsert )
    {
        Source = values;
        RecordSet = recordSet;
        InsertDataFields = insertDataFields;
        ConflictTarget = conflictTarget;
        UpdateSource = new SqlInternalRecordSetNode( recordSet );
        UpdateAssignments = updateAssignments( RecordSet, UpdateSource ).ToArray();
    }

    /// <summary>
    /// Source of records to be inserted or updated.
    /// </summary>
    /// <remarks>This can either be an <see cref="SqlValuesNode"/> or an <see cref="SqlQueryExpressionNode"/>.</remarks>
    public SqlNodeBase Source { get; }

    /// <summary>
    /// Table to upsert into.
    /// </summary>
    public SqlRecordSetNode RecordSet { get; }

    /// <summary>
    /// Source of records excluded from the insertion part of this upsert due to them already existing in the table.
    /// </summary>
    /// <remarks>This record set can be used in the update part of an upsert statement.</remarks>
    public SqlInternalRecordSetNode UpdateSource { get; }

    /// <summary>
    /// Collection of <see cref="RecordSet"/> data fields that the insertion part of this upsert refers to.
    /// </summary>
    public ReadOnlyArray<SqlDataFieldNode> InsertDataFields { get; }

    /// <summary>
    /// Collection of value assignments that the update part of this upsert refers to.
    /// </summary>
    public ReadOnlyArray<SqlValueAssignmentNode> UpdateAssignments { get; }

    /// <summary>
    /// Collection of data fields from the table that define the insertion conflict target.
    /// </summary>
    /// <remarks>Empty conflict target may cause the table's primary key to be used instead.</remarks>
    public ReadOnlyArray<SqlDataFieldNode> ConflictTarget { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
