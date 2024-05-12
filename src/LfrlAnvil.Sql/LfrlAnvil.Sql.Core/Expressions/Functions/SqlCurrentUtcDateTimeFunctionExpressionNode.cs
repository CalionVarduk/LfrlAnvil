using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns current datetime in UTC timezone.
/// </summary>
public sealed class SqlCurrentUtcDateTimeFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentUtcDateTimeFunctionExpressionNode()
        : base( SqlFunctionType.CurrentUtcDateTime, Array.Empty<SqlExpressionNode>() ) { }
}
