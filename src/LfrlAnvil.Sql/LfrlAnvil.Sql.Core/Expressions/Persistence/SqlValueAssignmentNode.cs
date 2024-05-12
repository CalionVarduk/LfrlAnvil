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
