namespace LfrlAnvil.Sql.Objects;

public interface ISqlObject
{
    string Name { get; }
    string FullName { get; }
    SqlObjectType Type { get; }
    ISqlDatabase Database { get; }
}
