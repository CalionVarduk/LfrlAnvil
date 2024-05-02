using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a provider of <see cref="DateTime"/> instances of <see cref="DateTimeKind.Utc"/> kind.
/// </summary>
public sealed class UtcDateTimeProvider : DateTimeProviderBase
{
    /// <summary>
    /// Creates a new <see cref="UtcDateTimeProvider"/> instance.
    /// </summary>
    public UtcDateTimeProvider()
        : base( DateTimeKind.Utc ) { }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override DateTime GetNow()
    {
        return DateTime.UtcNow;
    }
}
