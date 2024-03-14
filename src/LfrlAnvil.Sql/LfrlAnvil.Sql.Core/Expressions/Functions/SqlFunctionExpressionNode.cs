namespace LfrlAnvil.Sql.Expressions.Functions;

public abstract class SqlFunctionExpressionNode : SqlExpressionNode
{
    protected SqlFunctionExpressionNode(SqlExpressionNode[] arguments)
        : this( SqlFunctionType.Custom, arguments ) { }

    internal SqlFunctionExpressionNode(SqlFunctionType functionType, SqlExpressionNode[] arguments)
        : base( SqlNodeType.FunctionExpression )
    {
        Assume.IsDefined( functionType );
        FunctionType = functionType;
        Arguments = arguments;
    }

    public ReadOnlyArray<SqlExpressionNode> Arguments { get; }
    public SqlFunctionType FunctionType { get; }
}
