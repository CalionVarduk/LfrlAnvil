namespace LfrlAnvil.Sql;

/// <summary>
/// Represents an ordering strategy.
/// </summary>
public sealed class OrderBy : Enumeration<OrderBy, OrderBy.Values>
{
    /// <summary>
    /// Represents underlying <see cref="OrderBy"/> values.
    /// </summary>
    public enum Values : byte
    {
        /// <summary>
        /// <see cref="OrderBy.Asc"/> value.
        /// </summary>
        Asc = 0,

        /// <summary>
        /// <see cref="OrderBy.Desc"/> value.
        /// </summary>
        Desc = 1
    }

    /// <summary>
    /// Specifies that the ordering should be in ascending order.
    /// </summary>
    public static readonly OrderBy Asc = new OrderBy( "ASC", Values.Asc );

    /// <summary>
    /// Specifies that the ordering should be in descending order.
    /// </summary>
    public static readonly OrderBy Desc = new OrderBy( "DESC", Values.Desc );

    private OrderBy(string name, Values value)
        : base( name, value ) { }
}
