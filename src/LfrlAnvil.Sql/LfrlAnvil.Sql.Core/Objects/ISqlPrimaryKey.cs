namespace LfrlAnvil.Sql.Objects;

public interface ISqlPrimaryKey : ISqlConstraint
{
    ISqlIndex Index { get; }
}
