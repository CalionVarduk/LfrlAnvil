using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

public sealed class LocalDateTimeProvider : DateTimeProviderBase
{
    public LocalDateTimeProvider()
        : base( DateTimeKind.Local ) { }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override DateTime GetNow()
    {
        return DateTime.Now;
    }
}