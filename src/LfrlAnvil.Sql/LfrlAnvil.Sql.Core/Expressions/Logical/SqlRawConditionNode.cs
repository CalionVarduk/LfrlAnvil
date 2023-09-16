using System;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlRawConditionNode : SqlConditionNode
{
    internal SqlRawConditionNode(string sql, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawCondition )
    {
        Sql = sql;
        Parameters = parameters;
    }

    public string Sql { get; }
    public ReadOnlyMemory<SqlParameterNode> Parameters { get; }
}
