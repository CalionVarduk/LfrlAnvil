using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlViewDataFieldCollection : IReadOnlyCollection<ISqlViewDataField>
{
    ISqlView View { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlViewDataField Get(string name);

    [Pure]
    ISqlViewDataField? TryGet(string name);
}
