namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlLastIndexOfFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlLastIndexOfFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode value)
        : base( SqlFunctionType.LastIndexOf, new[] { argument, value } )
    {
        Type = argument.Type is null ? null : SqlExpressionType.Create<long>( isNullable: argument.Type.Value.IsNullable );
    }

    public override SqlExpressionType? Type { get; }
}
