namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlObjectBuilder
{
    string Name { get; }
    SqlObjectType Type { get; }
    ISqlDatabaseBuilder Database { get; }
    SqlObjectBuilderReferenceCollection<ISqlObjectBuilder> ReferencingObjects { get; }
    bool IsRemoved { get; }
    bool CanRemove { get; }

    ISqlObjectBuilder SetName(string name);
    void Remove();
}
