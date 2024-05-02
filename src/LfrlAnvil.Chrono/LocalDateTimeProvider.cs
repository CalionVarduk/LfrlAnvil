using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a provider of <see cref="DateTime"/> instances of <see cref="DateTimeKind.Local"/> kind.
/// </summary>
public sealed class LocalDateTimeProvider : DateTimeProviderBase
{
    /// <summary>
    /// Creates a new <see cref="LocalDateTimeProvider"/> instance.
    /// </summary>
    public LocalDateTimeProvider()
        : base( DateTimeKind.Local ) { }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override DateTime GetNow()
    {
        return DateTime.Now;
    }
}
