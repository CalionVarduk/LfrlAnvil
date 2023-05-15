using System.Collections.Generic;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlIndex : ISqlObject
{
    ISqlTable Table { get; }
    IReadOnlyList<ISqlIndexColumn> Columns { get; }
    bool IsUnique { get; }
}
