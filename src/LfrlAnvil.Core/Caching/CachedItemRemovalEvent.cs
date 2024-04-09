using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Caching;

public readonly record struct CachedItemRemovalEvent<TKey, TValue>(TKey Key, TValue Removed, TValue? Replacement, bool IsReplaced)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CachedItemRemovalEvent<TKey, TValue> CreateRemoved(TKey key, TValue removed)
    {
        return new CachedItemRemovalEvent<TKey, TValue>( key, removed, default, false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CachedItemRemovalEvent<TKey, TValue> CreateReplaced(TKey key, TValue removed, TValue replacement)
    {
        return new CachedItemRemovalEvent<TKey, TValue>( key, removed, replacement, true );
    }
}
