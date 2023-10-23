using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCheckDefinitionNode : SqlNodeBase
{
    internal SqlCheckDefinitionNode(string name, SqlConditionNode predicate)
        : base( SqlNodeType.CheckDefinition )
    {
        Name = name;
        Predicate = predicate;
    }

    public string Name { get; }
    public SqlConditionNode Predicate { get; }
}
