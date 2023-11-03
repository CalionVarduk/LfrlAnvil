namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlNamedFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlNamedFunctionExpressionNode(SqlSchemaObjectName name, SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Named, arguments )
    {
        Name = name;
    }

    public SqlSchemaObjectName Name { get; }
}
