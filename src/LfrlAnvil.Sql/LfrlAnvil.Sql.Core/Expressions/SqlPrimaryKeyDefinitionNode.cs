using System;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlPrimaryKeyDefinitionNode : SqlNodeBase
{
    internal SqlPrimaryKeyDefinitionNode(string name, SqlOrderByNode[] columns)
        : base( SqlNodeType.PrimaryKeyDefinition )
    {
        Name = name;
        Columns = columns;
    }

    public string Name { get; }
    public ReadOnlyMemory<SqlOrderByNode> Columns { get; }
}
