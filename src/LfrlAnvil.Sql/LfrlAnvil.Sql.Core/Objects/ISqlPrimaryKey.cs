namespace LfrlAnvil.Sql.Objects;

public interface ISqlPrimaryKey : ISqlObject
{
    ISqlIndex Index { get; }
}
