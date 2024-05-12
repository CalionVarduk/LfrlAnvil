using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns a new GUID.
/// </summary>
public sealed class SqlNewGuidFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlNewGuidFunctionExpressionNode()
        : base( SqlFunctionType.NewGuid, Array.Empty<SqlExpressionNode>() ) { }
}
