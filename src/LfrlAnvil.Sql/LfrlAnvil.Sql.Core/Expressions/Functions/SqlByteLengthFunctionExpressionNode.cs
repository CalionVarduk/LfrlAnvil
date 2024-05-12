namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that returns its parameter's length in bytes.
/// </summary>
public sealed class SqlByteLengthFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlByteLengthFunctionExpressionNode(SqlExpressionNode argument)
        : base( SqlFunctionType.ByteLength, new[] { argument } ) { }
}
