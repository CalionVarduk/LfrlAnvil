using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns current date in local timezone.
/// </summary>
public sealed class SqlCurrentDateFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentDateFunctionExpressionNode()
        : base( SqlFunctionType.CurrentDate, Array.Empty<SqlExpressionNode>() ) { }
}
