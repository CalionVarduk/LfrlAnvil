using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCheckDefinitionNode : SqlNodeBase
{
    internal SqlCheckDefinitionNode(string name, SqlConditionNode condition)
        : base( SqlNodeType.CheckDefinition )
    {
        Name = name;
        Condition = condition;
    }

    public string Name { get; }
    public SqlConditionNode Condition { get; }
}
