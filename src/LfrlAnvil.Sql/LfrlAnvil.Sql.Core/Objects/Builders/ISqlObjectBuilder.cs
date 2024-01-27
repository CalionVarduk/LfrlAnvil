namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlObjectBuilder
{
    string Name { get; }
    SqlObjectType Type { get; }
    ISqlDatabaseBuilder Database { get; }
    bool IsRemoved { get; }

    ISqlObjectBuilder SetName(string name);
    void Remove();
}
