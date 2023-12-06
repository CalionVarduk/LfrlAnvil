using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlQueryReaderOptions(int? InitialBufferCapacity)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public List<TRow> CreateList<TRow>()
    {
        return InitialBufferCapacity.HasValue
            ? new List<TRow>( capacity: InitialBufferCapacity.Value )
            : new List<TRow>();
    }
}
