namespace LfrlAnvil.Sql.Objects;

public interface ISqlObject
{
    string Name { get; }
    SqlObjectType Type { get; }
    ISqlDatabase Database { get; }
}
