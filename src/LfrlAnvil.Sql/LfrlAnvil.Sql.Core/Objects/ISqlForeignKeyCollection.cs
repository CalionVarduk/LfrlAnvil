using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlForeignKeyCollection : IReadOnlyCollection<ISqlForeignKey>
{
    ISqlTable Table { get; }

    [Pure]
    bool Contains(ISqlIndex originIndex, ISqlIndex referencedIndex);

    [Pure]
    ISqlForeignKey Get(ISqlIndex originIndex, ISqlIndex referencedIndex);

    bool TryGet(ISqlIndex originIndex, ISqlIndex referencedIndex, [MaybeNullWhen( false )] out ISqlForeignKey result);
}
