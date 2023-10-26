using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlViewDataField : ISqlObject
{
    ISqlView View { get; }
    SqlViewDataFieldNode Node { get; }
}
