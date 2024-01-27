using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCheckDefinitionNode : SqlNodeBase
{
    internal SqlCheckDefinitionNode(SqlSchemaObjectName name, SqlConditionNode condition)
        : base( SqlNodeType.CheckDefinition )
    {
        Name = name;
        Condition = condition;
    }

    public SqlSchemaObjectName Name { get; }
    public SqlConditionNode Condition { get; }
}
