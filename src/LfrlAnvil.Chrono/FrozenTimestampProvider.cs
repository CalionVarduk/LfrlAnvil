using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a <see cref="Timestamp"/> provider with a single frozen value.
/// </summary>
public sealed class FrozenTimestampProvider : TimestampProviderBase
{
    private readonly Timestamp _now;

    /// <summary>
    /// Creates a new <see cref="FrozenTimestampProvider"/> instance.
    /// </summary>
    /// <param name="now">Stored <see cref="Timestamp"/> returned by this instance.</param>
    public FrozenTimestampProvider(Timestamp now)
    {
        _now = now;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override Timestamp GetNow()
    {
        return _now;
    }
}
