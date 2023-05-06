namespace LfrlAnvil.Sql.Builders;

public interface ISqlIndexColumnBuilder
{
    ISqlColumnBuilder Column { get; }
    OrderBy Ordering { get; }
}
