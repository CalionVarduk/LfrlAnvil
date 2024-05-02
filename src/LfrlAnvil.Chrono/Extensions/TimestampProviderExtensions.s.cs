using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="ITimestampProvider"/> extension methods.
/// </summary>
public static class TimestampProviderExtensions
{
    /// <summary>
    /// Checks whether or not the provided <paramref name="timestamp"/> is in the past
    /// relative to the given timestamp <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Source timestamp provider.</param>
    /// <param name="timestamp">Timestamp to check.</param>
    /// <returns><b>true</b> when <paramref name="timestamp"/> is in the past, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsInPast(this ITimestampProvider provider, Timestamp timestamp)
    {
        return timestamp < provider.GetNow();
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="timestamp"/> is in the present
    /// relative to the given timestamp <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Source timestamp provider.</param>
    /// <param name="timestamp">Timestamp to check.</param>
    /// <returns><b>true</b> when <paramref name="timestamp"/> is in the present, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNow(this ITimestampProvider provider, Timestamp timestamp)
    {
        return timestamp == provider.GetNow();
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="timestamp"/> is in the future
    /// relative to the given timestamp <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Source timestamp provider.</param>
    /// <param name="timestamp">Timestamp to check.</param>
    /// <returns><b>true</b> when <paramref name="timestamp"/> is in the future, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsInFuture(this ITimestampProvider provider, Timestamp timestamp)
    {
        return timestamp > provider.GetNow();
    }

    /// <summary>
    /// Checks whether or not the given timestamp <paramref name="provider"/> is frozen.
    /// </summary>
    /// <param name="provider">Timestamp provider to check.</param>
    /// <returns><b>true</b> when <paramref name="provider"/> is frozen, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsFrozen(this ITimestampProvider provider)
    {
        return provider is FrozenTimestampProvider;
    }

    /// <summary>
    /// Calculates an offset between the provided <paramref name="timestamp"/>
    /// and the current <see cref="Timestamp"/> of the given timestamp <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Source timestamp provider.</param>
    /// <param name="timestamp">Timestamp to check.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration GetDifference(this ITimestampProvider provider, Timestamp timestamp)
    {
        return timestamp - provider.GetNow();
    }

    /// <summary>
    /// Calculates an offset between current <see cref="Timestamp"/> instances of two timestamp providers.
    /// </summary>
    /// <param name="provider">Source timestamp provider.</param>
    /// <param name="other">Other timestamp provider to check.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration GetDifference(this ITimestampProvider provider, ITimestampProvider other)
    {
        return provider.GetDifference( other.GetNow() );
    }

    /// <summary>
    /// Creates a new frozen <see cref="ITimestampProvider"/> instance from the given <paramref name="provider"/>,
    /// using its current <see cref="Timestamp"/>.
    /// </summary>
    /// <param name="provider">Source timestamp provider.</param>
    /// <returns>New frozen <see cref="ITimestampProvider"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ITimestampProvider Freeze(this ITimestampProvider provider)
    {
        return new FrozenTimestampProvider( provider.GetNow() );
    }
}
