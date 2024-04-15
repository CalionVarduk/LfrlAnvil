using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Caching;

namespace LfrlAnvil.Chrono.Extensions;

public static class LifetimeCacheExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void MoveTo<TKey, TValue>(this ILifetimeCache<TKey, TValue> cache, Timestamp timestamp)
        where TKey : notnull
    {
        cache.Move( timestamp - cache.CurrentTimestamp );
    }
}
