namespace LfrlAnvil.Sql.Objects;

public interface ISqlConstraint : ISqlObject
{
    ISqlTable Table { get; }
}
