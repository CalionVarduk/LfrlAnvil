namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlIndexOfFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlIndexOfFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode value)
        : base( SqlFunctionType.IndexOf, new[] { argument, value } )
    {
        Type = argument.Type is null ? null : SqlExpressionType.Create<long>( isNullable: argument.Type.Value.IsNullable );
    }

    public override SqlExpressionType? Type { get; }
}
