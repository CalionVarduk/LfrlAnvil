namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a function invocation.
/// </summary>
public abstract class SqlFunctionExpressionNode : SqlExpressionNode
{
    /// <summary>
    /// Creates a new <see cref="SqlFunctionExpressionNode"/> instance with <see cref="SqlFunctionType.Custom"/> type.
    /// </summary>
    /// <param name="arguments">Sequential collection of invocation arguments.</param>
    protected SqlFunctionExpressionNode(SqlExpressionNode[] arguments)
        : this( SqlFunctionType.Custom, arguments ) { }

    internal SqlFunctionExpressionNode(SqlFunctionType functionType, SqlExpressionNode[] arguments)
        : base( SqlNodeType.FunctionExpression )
    {
        Assume.IsDefined( functionType );
        FunctionType = functionType;
        Arguments = arguments;
    }

    /// <summary>
    /// Sequential collection of invocation arguments.
    /// </summary>
    public ReadOnlyArray<SqlExpressionNode> Arguments { get; }

    /// <summary>
    /// <see cref="SqlFunctionType"/> of this function.
    /// </summary>
    public SqlFunctionType FunctionType { get; }
}
