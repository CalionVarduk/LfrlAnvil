using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

/// <inheritdoc cref="TimestampProviderBase" />
public sealed class TimestampProvider : TimestampProviderBase
{
    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override Timestamp GetNow()
    {
        return new Timestamp( DateTime.UtcNow );
    }
}
