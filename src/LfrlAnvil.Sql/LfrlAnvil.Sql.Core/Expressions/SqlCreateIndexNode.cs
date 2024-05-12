using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines creation of a single index.
/// </summary>
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

    /// <summary>
    /// Index's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <summary>
    /// Specifies whether or not this index is unique.
    /// </summary>
    public bool IsUnique { get; }

    /// <summary>
    /// Specifies whether or not the index should be replaced if it already exists in DB.
    /// </summary>
    public bool ReplaceIfExists { get; }

    /// <summary>
    /// Table on which this index is created.
    /// </summary>
    public SqlRecordSetNode Table { get; }

    /// <summary>
    /// Collection of expressions that define this index.
    /// </summary>
    public ReadOnlyArray<SqlOrderByNode> Columns { get; }

    /// <summary>
    /// Optional filter condition.
    /// </summary>
    public SqlConditionNode? Filter { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
