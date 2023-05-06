namespace LfrlAnvil.Sql;

public sealed class OrderBy : Enumeration<OrderBy, OrderBy.Values>
{
    public enum Values : byte
    {
        Asc = 0,
        Desc = 1
    }

    public static readonly OrderBy Asc = new OrderBy( "ASC", Values.Asc );
    public static readonly OrderBy Desc = new OrderBy( "DESC", Values.Desc );

    private OrderBy(string name, Values value)
        : base( name, value ) { }
}
