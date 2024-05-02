using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a <see cref="DateTime"/> provider with a single frozen value.
/// </summary>
public sealed class FrozenDateTimeProvider : DateTimeProviderBase
{
    private readonly DateTime _now;

    /// <summary>
    /// Creates a new <see cref="FrozenDateTimeProvider"/> instance.
    /// </summary>
    /// <param name="now">Stored <see cref="DateTime"/> returned by this instance.</param>
    public FrozenDateTimeProvider(DateTime now)
        : base( now.Kind )
    {
        _now = now;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override DateTime GetNow()
    {
        return _now;
    }
}
