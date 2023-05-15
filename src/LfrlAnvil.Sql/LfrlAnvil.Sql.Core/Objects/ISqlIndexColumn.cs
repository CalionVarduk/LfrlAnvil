namespace LfrlAnvil.Sql.Objects;

public interface ISqlIndexColumn
{
    ISqlColumn Column { get; }
    OrderBy Ordering { get; }
}
