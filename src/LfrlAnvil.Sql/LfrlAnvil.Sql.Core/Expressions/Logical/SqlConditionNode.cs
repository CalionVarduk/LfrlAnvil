using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Logical;

public abstract class SqlConditionNode : SqlNodeBase
{
    protected SqlConditionNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    [Pure]
    public static SqlAndConditionNode operator &(SqlConditionNode left, SqlConditionNode right)
    {
        return left.And( right );
    }

    [Pure]
    public static SqlOrConditionNode operator |(SqlConditionNode left, SqlConditionNode right)
    {
        return left.Or( right );
    }
}
