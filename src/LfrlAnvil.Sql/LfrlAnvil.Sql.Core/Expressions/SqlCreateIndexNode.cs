using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateIndexNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlCreateIndexNode(
        SqlSchemaObjectName name,
        bool isUnique,
        bool replaceIfExists,
        SqlRecordSetNode table,
        ReadOnlyArray<SqlOrderByNode> columns,
        SqlConditionNode? filter)
        : base( SqlNodeType.CreateIndex )
    {
        Name = name;
        IsUnique = isUnique;
        ReplaceIfExists = replaceIfExists;
        Table = table;
        Columns = columns;
        Filter = filter;
    }

    public SqlSchemaObjectName Name { get; }
    public bool IsUnique { get; }
    public bool ReplaceIfExists { get; }
    public SqlRecordSetNode Table { get; }
    public ReadOnlyArray<SqlOrderByNode> Columns { get; }
    public SqlConditionNode? Filter { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
