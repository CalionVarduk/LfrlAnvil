using System;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateIndexNode : SqlNodeBase
{
    internal SqlCreateIndexNode(
        SqlSchemaObjectName name,
        bool isUnique,
        bool ifNotExists,
        SqlRecordSetNode table,
        SqlOrderByNode[] columns,
        SqlConditionNode? filter)
        : base( SqlNodeType.CreateIndex )
    {
        Name = name;
        IsUnique = isUnique;
        IfNotExists = ifNotExists;
        Table = table;
        Columns = columns;
        Filter = filter;
    }

    public SqlSchemaObjectName Name { get; }
    public bool IsUnique { get; }
    public bool IfNotExists { get; }
    public SqlRecordSetNode Table { get; }
    public ReadOnlyMemory<SqlOrderByNode> Columns { get; }
    public SqlConditionNode? Filter { get; }
}
