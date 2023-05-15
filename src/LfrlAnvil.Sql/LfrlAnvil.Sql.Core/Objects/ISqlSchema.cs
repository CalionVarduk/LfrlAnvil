namespace LfrlAnvil.Sql.Objects;

public interface ISqlSchema : ISqlObject
{
    ISqlObjectCollection Objects { get; }
}
