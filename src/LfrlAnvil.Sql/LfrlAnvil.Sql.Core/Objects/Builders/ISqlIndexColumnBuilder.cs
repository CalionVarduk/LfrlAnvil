namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlIndexColumnBuilder
{
    ISqlColumnBuilder Column { get; }
    OrderBy Ordering { get; }
}
