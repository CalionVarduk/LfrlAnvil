using System;
using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

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

    public SqlNodeBase Source { get; }
    public SqlRecordSetNode RecordSet { get; }
    public SqlInternalRecordSetNode UpdateSource { get; }
    public ReadOnlyArray<SqlDataFieldNode> InsertDataFields { get; }
    public ReadOnlyArray<SqlValueAssignmentNode> UpdateAssignments { get; }
    public ReadOnlyArray<SqlDataFieldNode> ConflictTarget { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
