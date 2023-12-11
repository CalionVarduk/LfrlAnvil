namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlByteLengthFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlByteLengthFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ByteLength, new[] { argument } ) { }
}
