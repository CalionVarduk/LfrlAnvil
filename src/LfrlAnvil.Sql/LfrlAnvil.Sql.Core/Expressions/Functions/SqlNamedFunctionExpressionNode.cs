namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a custom function.
/// </summary>
public sealed class SqlNamedFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlNamedFunctionExpressionNode(SqlSchemaObjectName name, SqlExpressionNode[] arguments)
        : base( SqlFunctionType.Named, arguments )
    {
        Name = name;
    }

    /// <summary>
    /// Function's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }
}
