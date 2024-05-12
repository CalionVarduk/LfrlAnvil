using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns current datetime in local timezone.
/// </summary>
public sealed class SqlCurrentDateTimeFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentDateTimeFunctionExpressionNode()
        : base( SqlFunctionType.CurrentDateTime, Array.Empty<SqlExpressionNode>() ) { }
}
