using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node that defines a check constraint.
/// </summary>
public sealed class SqlCheckDefinitionNode : SqlNodeBase
{
    internal SqlCheckDefinitionNode(SqlSchemaObjectName name, SqlConditionNode condition)
        : base( SqlNodeType.CheckDefinition )
    {
        Name = name;
        Condition = condition;
    }

    /// <summary>
    /// Check constraint's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <summary>
    /// Check constraint's condition.
    /// </summary>
    public SqlConditionNode Condition { get; }
}
