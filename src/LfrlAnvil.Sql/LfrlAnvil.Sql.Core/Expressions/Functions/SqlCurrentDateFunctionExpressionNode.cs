using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCurrentDateFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlCurrentDateFunctionExpressionNode()
        : base( SqlFunctionType.CurrentDate, Array.Empty<SqlExpressionNode>() ) { }
}
