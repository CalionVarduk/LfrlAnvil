using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlViewDataFieldCollection : IReadOnlyCollection<ISqlViewDataField>
{
    ISqlView View { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlViewDataField GetField(string name);

    [Pure]
    ISqlViewDataField? TryGetField(string name);
}
