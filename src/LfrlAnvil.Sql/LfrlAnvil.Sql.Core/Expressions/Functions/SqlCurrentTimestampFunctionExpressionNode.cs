using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns current time elapsed
/// since the unix epoch.
/// </summary>
public sealed class SqlCurrentTimestampFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentTimestampFunctionExpressionNode()
        : base( SqlFunctionType.CurrentTimestamp, Array.Empty<SqlExpressionNode>() ) { }
}
