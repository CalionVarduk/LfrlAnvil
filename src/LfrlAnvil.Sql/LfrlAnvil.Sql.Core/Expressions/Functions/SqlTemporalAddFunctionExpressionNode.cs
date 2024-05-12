namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that adds a value with a given unit
/// to the date and/or time parameter.
/// </summary>
public sealed class SqlTemporalAddFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlTemporalAddFunctionExpressionNode(SqlExpressionNode argument, SqlExpressionNode value, SqlTemporalUnit unit)
        : base( SqlFunctionType.TemporalAdd, new[] { argument, value } )
    {
        Ensure.IsDefined( unit );
        Unit = unit;
    }

    /// <summary>
    /// <see cref="SqlTemporalUnit"/> that specifies the unit of the added value.
    /// </summary>
    public SqlTemporalUnit Unit { get; }
}
