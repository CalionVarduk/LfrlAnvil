using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents <see cref="SqlQueryReader"/> options.
/// </summary>
/// <param name="InitialBufferCapacity">Specifies the initial capacity of read rows buffer.</param>
public readonly record struct SqlQueryReaderOptions(int? InitialBufferCapacity)
{
    /// <summary>
    /// Creates an initial rows buffer.
    /// </summary>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>New <see cref="List{T}"/> instance.</returns>
    /// <remarks>
    /// When <see cref="InitialBufferCapacity"/> is not null,
    /// then it will be used to set the initial <see cref="List{T}.Capacity"/> of the returned buffer.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public List<TRow> CreateList<TRow>()
    {
        return InitialBufferCapacity.HasValue
            ? new List<TRow>( capacity: InitialBufferCapacity.Value )
            : new List<TRow>();
    }
}
