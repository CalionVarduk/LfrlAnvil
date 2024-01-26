using System;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlIndex : ISqlConstraint
{
    ReadOnlyMemory<ISqlIndexColumn> Columns { get; }
    bool IsUnique { get; }
    bool IsPartial { get; }
}
