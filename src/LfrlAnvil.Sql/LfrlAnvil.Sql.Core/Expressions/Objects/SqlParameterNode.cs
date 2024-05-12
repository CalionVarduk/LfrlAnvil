namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a single bound parameter.
/// </summary>
public sealed class SqlParameterNode : SqlExpressionNode
{
    internal SqlParameterNode(string name, TypeNullability? type, int? index)
        : base( SqlNodeType.Parameter )
    {
        if ( index is not null )
            Ensure.IsGreaterThanOrEqualTo( index.Value, 0 );

        Name = name;
        Type = type;
        Index = index;
    }

    /// <summary>
    /// Parameter's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Optional runtime type of this parameter.
    /// </summary>
    public TypeNullability? Type { get; }

    /// <summary>
    /// Optional 0-based position of this parameter.
    /// </summary>
    /// <remarks>Non-null values mean that the parameter may be interpreted as a positional parameter.</remarks>
    public int? Index { get; }
}
