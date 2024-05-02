using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a <see cref="ZonedDateTime"/> provider with a single frozen value.
/// </summary>
public sealed class FrozenZonedClock : ZonedClockBase
{
    private readonly ZonedDateTime _now;

    /// <summary>
    /// Creates a new <see cref="FrozenZonedClock"/> instance.
    /// </summary>
    /// <param name="now">Stored <see cref="ZonedDateTime"/> returned by this instance.</param>
    public FrozenZonedClock(ZonedDateTime now)
        : base( now.TimeZone )
    {
        _now = now;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override ZonedDateTime GetNow()
    {
        return _now;
    }
}
