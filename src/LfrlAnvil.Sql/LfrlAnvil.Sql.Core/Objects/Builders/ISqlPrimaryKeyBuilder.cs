namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlPrimaryKeyBuilder : ISqlConstraintBuilder
{
    ISqlIndexBuilder Index { get; }

    new ISqlPrimaryKeyBuilder SetName(string name);
    new ISqlPrimaryKeyBuilder SetDefaultName();
}
