namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a function that extracts a desired date or time component
/// from its parameter.
/// </summary>
public sealed class SqlExtractTemporalUnitFunctionExpressionNode : SqlFunctionExpressionNode
{
    internal SqlExtractTemporalUnitFunctionExpressionNode(SqlExpressionNode argument, SqlTemporalUnit unit)
        : base( SqlFunctionType.ExtractTemporalUnit, new[] { argument } )
    {
        Ensure.IsDefined( unit );
        Unit = unit;
    }

    /// <summary>
    /// <see cref="SqlTemporalUnit"/> that specifies the date or time component to extract.
    /// </summary>
    public SqlTemporalUnit Unit { get; }
}
