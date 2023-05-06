namespace LfrlAnvil.Sql.Builders;

public interface ISqlObjectBuilder
{
    string Name { get; }
    string FullName { get; }
    SqlObjectType Type { get; }
    ISqlDatabaseBuilder Database { get; }
    bool IsRemoved { get; }

    ISqlObjectBuilder SetName(string name);
    void Remove();
}
