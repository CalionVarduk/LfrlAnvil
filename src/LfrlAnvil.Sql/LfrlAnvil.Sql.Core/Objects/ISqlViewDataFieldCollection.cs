using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlViewDataFieldCollection : IReadOnlyCollection<ISqlViewDataField>
{
    ISqlView View { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlViewDataField Get(string name);

    bool TryGet(string name, [MaybeNullWhen( false )] out ISqlViewDataField result);
}
