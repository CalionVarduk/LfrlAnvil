using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlSchemaCollection : IReadOnlyCollection<ISqlSchema>
{
    ISqlDatabase Database { get; }
    ISqlSchema Default { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlSchema Get(string name);

    [Pure]
    ISqlSchema? TryGet(string name);
}
