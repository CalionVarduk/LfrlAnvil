using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlColumnCollection : IReadOnlyCollection<ISqlColumn>
{
    ISqlTable Table { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlColumn Get(string name);

    [Pure]
    ISqlColumn? TryGet(string name);
}
