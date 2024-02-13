using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlView : ISqlObject
{
    ISqlSchema Schema { get; }
    ISqlViewDataFieldCollection DataFields { get; }
    SqlRecordSetInfo Info { get; }
    SqlViewNode Node { get; }
}
