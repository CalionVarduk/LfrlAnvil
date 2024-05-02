using System;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Contains instances of <see cref="IDateTimeProvider"/> type.
/// </summary>
public static class DateTimeProvider
{
    /// <summary>
    /// <see cref="IDateTimeProvider"/> instance that returns <see cref="DateTime"/> instances of <see cref="DateTimeKind.Utc"/> kind.
    /// </summary>
    public static readonly IDateTimeProvider Utc = new UtcDateTimeProvider();

    /// <summary>
    /// <see cref="IDateTimeProvider"/> instance that returns <see cref="DateTime"/> instances of <see cref="DateTimeKind.Local"/> kind.
    /// </summary>
    public static readonly IDateTimeProvider Local = new LocalDateTimeProvider();
}
