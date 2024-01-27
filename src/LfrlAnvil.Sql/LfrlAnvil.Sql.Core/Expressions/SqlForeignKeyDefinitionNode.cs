using System;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

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

    public SqlSchemaObjectName Name { get; }
    public ReadOnlyMemory<SqlDataFieldNode> Columns { get; }
    public SqlRecordSetNode ReferencedTable { get; }
    public ReadOnlyMemory<SqlDataFieldNode> ReferencedColumns { get; }
    public ReferenceBehavior OnDeleteBehavior { get; }
    public ReferenceBehavior OnUpdateBehavior { get; }
}
