using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node that defines a foreign key constraint.
/// </summary>
public sealed class SqlForeignKeyDefinitionNode : SqlNodeBase
{
    internal SqlForeignKeyDefinitionNode(
        SqlSchemaObjectName name,
        SqlDataFieldNode[] columns,
        SqlRecordSetNode referencedTable,
        SqlDataFieldNode[] referencedColumns,
        ReferenceBehavior onDeleteBehavior,
        ReferenceBehavior onUpdateBehavior)
        : base( SqlNodeType.ForeignKeyDefinition )
    {
        Name = name;
        Columns = columns;
        ReferencedTable = referencedTable;
        ReferencedColumns = referencedColumns;
        OnDeleteBehavior = onDeleteBehavior;
        OnUpdateBehavior = onUpdateBehavior;
    }

    /// <summary>
    /// Foreign key constraint's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <summary>
    /// Collection of columns from source table that this foreign key originates from.
    /// </summary>
    public ReadOnlyArray<SqlDataFieldNode> Columns { get; }

    /// <summary>
    /// Table referenced by this foreign key constraint.
    /// </summary>
    public SqlRecordSetNode ReferencedTable { get; }

    /// <summary>
    /// Collection of columns from <see cref="ReferencedTable"/> referenced by this foreign key constraint.
    /// </summary>
    public ReadOnlyArray<SqlDataFieldNode> ReferencedColumns { get; }

    /// <summary>
    /// Specifies this foreign key constraint's on delete behavior.
    /// </summary>
    public ReferenceBehavior OnDeleteBehavior { get; }

    /// <summary>
    /// Specifies this foreign key constraint's on update behavior.
    /// </summary>
    public ReferenceBehavior OnUpdateBehavior { get; }
}
