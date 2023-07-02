namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlSignFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlSignFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Sign, new[] { argument } )
    {
        Type = argument.Type is null ? null : SqlExpressionType.Create<int>( isNullable: argument.Type.Value.IsNullable );
    }

    public override SqlExpressionType? Type { get; }
}
