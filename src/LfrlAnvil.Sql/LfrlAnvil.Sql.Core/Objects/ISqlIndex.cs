using System.Collections.Generic;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlIndex : ISqlConstraint
{
    IReadOnlyList<SqlIndexColumn<ISqlColumn>> Columns { get; }
    bool IsUnique { get; }
    bool IsPartial { get; }
}
