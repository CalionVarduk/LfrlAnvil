using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

/// <summary>
/// Represents an SQL syntax tree statement node that defines an insertion of new records to a table.
/// </summary>
public sealed class SqlInsertIntoNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlInsertIntoNode(SqlQueryExpressionNode query, SqlRecordSetNode recordSet, SqlDataFieldNode[] dataFields)
        : base( SqlNodeType.InsertInto )
    {
        Source = query;
        RecordSet = recordSet;
        DataFields = dataFields;
    }

    internal SqlInsertIntoNode(SqlValuesNode values, SqlRecordSetNode recordSet, SqlDataFieldNode[] dataFields)
        : base( SqlNodeType.InsertInto )
    {
        Source = values;
        RecordSet = recordSet;
        DataFields = dataFields;
    }

    /// <summary>
    /// Source of records to be inserted.
    /// </summary>
    /// <remarks>This can either be an <see cref="SqlValuesNode"/> or an <see cref="SqlQueryExpressionNode"/>.</remarks>
    public SqlNodeBase Source { get; }

    /// <summary>
    /// Table to insert into.
    /// </summary>
    public SqlRecordSetNode RecordSet { get; }

    /// <summary>
    /// Collection of <see cref="RecordSet"/> data fields that this insertion refers to.
    /// </summary>
    public ReadOnlyArray<SqlDataFieldNode> DataFields { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
