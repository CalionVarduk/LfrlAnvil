using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public abstract class SqlFunctionExpressionNode : SqlExpressionNode
{
    protected SqlFunctionExpressionNode(SqlFunctionType functionType, SqlExpressionNode[] arguments)
        : base( SqlNodeType.FunctionExpression )
    {
        FunctionType = functionType;
        Arguments = arguments;
    }

    public ReadOnlyMemory<SqlExpressionNode> Arguments { get; }
    public SqlFunctionType FunctionType { get; }
}
