using System;

namespace LfrlAnvil.Sql.Expressions.Functions;

public abstract class SqlFunctionExpressionNode : SqlExpressionNode
{
    protected SqlFunctionExpressionNode(SqlExpressionNode[] arguments)
        : this( SqlFunctionType.Custom, arguments ) { }

    internal SqlFunctionExpressionNode(SqlFunctionType functionType, SqlExpressionNode[] arguments)
        : base( SqlNodeType.FunctionExpression )
    {
        Assume.IsDefined( functionType, nameof( functionType ) );
        FunctionType = functionType;
        Arguments = arguments;
    }

    public ReadOnlyMemory<SqlExpressionNode> Arguments { get; }
    public SqlFunctionType FunctionType { get; }
}
