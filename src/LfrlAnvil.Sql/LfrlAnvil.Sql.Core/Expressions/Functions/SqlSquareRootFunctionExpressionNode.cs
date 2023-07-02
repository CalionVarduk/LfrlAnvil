namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlSquareRootFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlSquareRootFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.SquareRoot, new[] { argument } )
    {
        Type = argument.Type is null ? null : SqlExpressionType.Create<double>( isNullable: argument.Type.Value.IsNullable );
    }

    public override SqlExpressionType? Type { get; }
}
