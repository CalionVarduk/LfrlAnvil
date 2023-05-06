namespace LfrlAnvil.Sql.Builders;

public interface ISqlPrimaryKeyBuilder : ISqlObjectBuilder
{
    ISqlIndexBuilder Index { get; }

    new ISqlPrimaryKeyBuilder SetName(string name);
    ISqlPrimaryKeyBuilder SetDefaultName();
}
