using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlCheckCollection : IReadOnlyCollection<ISqlCheck>
{
    ISqlTable Table { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlCheck Get(string name);

    bool TryGet(string name, [MaybeNullWhen( false )] out ISqlCheck result);
}
