using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

public sealed class FrozenZonedClock : ZonedClockBase
{
    private readonly ZonedDateTime _now;

    public FrozenZonedClock(ZonedDateTime now)
        : base( now.TimeZone )
    {
        _now = now;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override ZonedDateTime GetNow()
    {
        return _now;
    }
}
