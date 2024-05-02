using System;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Contains instances of precise <see cref="IDateTimeProvider"/> type.
/// </summary>
public static class PreciseDateTimeProvider
{
    /// <summary>
    /// <see cref="IDateTimeProvider"/> instance that returns precise <see cref="DateTime"/> instances
    /// of <see cref="DateTimeKind.Utc"/> kind, with <see cref="PreciseUtcDateTimeProvider.PrecisionResetTimeout"/>
    /// equal to <b>1 minute</b>.
    /// </summary>
    public static readonly IDateTimeProvider Utc = new PreciseUtcDateTimeProvider();

    /// <summary>
    /// <see cref="IDateTimeProvider"/> instance that returns precise <see cref="DateTime"/> instances
    /// of <see cref="DateTimeKind.Local"/> kind, with <see cref="PreciseLocalDateTimeProvider.PrecisionResetTimeout"/>
    /// equal to <b>1 minute</b>.
    /// </summary>
    public static readonly IDateTimeProvider Local = new PreciseLocalDateTimeProvider();
}
