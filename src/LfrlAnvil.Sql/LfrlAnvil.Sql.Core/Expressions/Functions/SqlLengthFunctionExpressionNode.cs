namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlLengthFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlLengthFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.Length, new[] { argument } )
    {
        Type = argument.Type is null ? null : SqlExpressionType.Create<long>( isNullable: argument.Type.Value.IsNullable );
    }

    public override SqlExpressionType? Type { get; }
}
