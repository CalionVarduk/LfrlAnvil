using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlNewGuidFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlNewGuidFunctionExpressionNode()
        : base( SqlFunctionType.NewGuid, Array.Empty<SqlExpressionNode>() ) { }
}
