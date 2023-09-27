namespace LfrlAnvil.Sql.Objects;

public interface ISqlView : ISqlObject
{
    ISqlSchema Schema { get; }
    ISqlViewDataFieldCollection DataFields { get; }
}
