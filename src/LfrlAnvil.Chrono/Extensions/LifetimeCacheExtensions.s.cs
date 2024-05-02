using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Caching;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="ILifetimeCache{TKey,TValue}"/> extension methods.
/// </summary>
public static class LifetimeCacheExtensions
{
    /// <summary>
    /// Moves the provided <paramref name="cache"/> to the given <paramref name="timestamp"/>.
    /// </summary>
    /// <param name="cache">Source cache.</param>
    /// <param name="timestamp"><see cref="Timestamp"/> to move the cache to.</param>
    /// <typeparam name="TKey">Cache's key type.</typeparam>
    /// <typeparam name="TValue">Cache's value type.</typeparam>
    /// <remarks>See <see cref="ILifetimeCache{TKey,TValue}.Move(Duration)"/> for more information.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void MoveTo<TKey, TValue>(this ILifetimeCache<TKey, TValue> cache, Timestamp timestamp)
        where TKey : notnull
    {
        cache.Move( timestamp - cache.CurrentTimestamp );
    }
}
