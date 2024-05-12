using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns current time of day
/// in local timezone.
/// </summary>
public sealed class SqlCurrentTimeFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentTimeFunctionExpressionNode()
        : base( SqlFunctionType.CurrentTime, Array.Empty<SqlExpressionNode>() ) { }
}
