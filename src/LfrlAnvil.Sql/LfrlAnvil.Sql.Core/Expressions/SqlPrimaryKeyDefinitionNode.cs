using System;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlPrimaryKeyDefinitionNode : SqlNodeBase
{
    internal SqlPrimaryKeyDefinitionNode(SqlSchemaObjectName name, SqlOrderByNode[] columns)
        : base( SqlNodeType.PrimaryKeyDefinition )
    {
        Name = name;
        Columns = columns;
    }

    public SqlSchemaObjectName Name { get; }
    public ReadOnlyMemory<SqlOrderByNode> Columns { get; }
}
