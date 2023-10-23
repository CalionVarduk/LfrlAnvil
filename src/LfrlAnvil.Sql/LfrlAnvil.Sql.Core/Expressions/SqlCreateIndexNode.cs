using System;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateIndexNode : SqlNodeBase
{
    internal SqlCreateIndexNode(
        string schemaName,
        string name,
        bool isUnique,
        bool ifNotExists,
        SqlRecordSetNode table,
        SqlOrderByNode[] columns,
        SqlConditionNode? filter)
        : base( SqlNodeType.CreateIndex )
    {
        SchemaName = schemaName;
        Name = name;
        IsUnique = isUnique;
        IfNotExists = ifNotExists;
        Table = table;
        Columns = columns;
        Filter = filter;
    }

    public string SchemaName { get; }
    public string Name { get; }
    public bool IsUnique { get; }
    public bool IfNotExists { get; }
    public SqlRecordSetNode Table { get; }
    public ReadOnlyMemory<SqlOrderByNode> Columns { get; }
    public SqlConditionNode? Filter { get; }
}
