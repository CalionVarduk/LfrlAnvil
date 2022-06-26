using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

public sealed class FrozenDateTimeProvider : DateTimeProviderBase
{
    private readonly DateTime _now;

    public FrozenDateTimeProvider(DateTime now)
        : base( now.Kind )
    {
        _now = now;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override DateTime GetNow()
    {
        return _now;
    }
}
