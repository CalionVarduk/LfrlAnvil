using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

public interface ISqlIndexCollection : IReadOnlyCollection<ISqlIndex>
{
    ISqlTable Table { get; }

    [Pure]
    bool Contains(ReadOnlyMemory<ISqlIndexColumn> columns);

    [Pure]
    ISqlIndex Get(ReadOnlyMemory<ISqlIndexColumn> columns);

    bool TryGet(ReadOnlyMemory<ISqlIndexColumn> columns, [MaybeNullWhen( false )] out ISqlIndex result);
}
