using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlColumnCollection : IReadOnlyCollection<ISqlColumn>
{
    ISqlTable Table { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlColumn GetColumn(string name);

    [Pure]
    ISqlColumn? TryGetColumn(string name);
}
