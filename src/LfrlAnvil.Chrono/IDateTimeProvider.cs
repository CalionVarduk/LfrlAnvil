using System;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a provider of <see cref="DateTime"/> instances.
/// </summary>
public interface IDateTimeProvider : IGenerator<DateTime>
{
    /// <summary>
    /// Specifies the resulting <see cref="DateTimeKind"/> of created instances.
    /// </summary>
    DateTimeKind Kind { get; }

    /// <summary>
    /// Returns the current <see cref="DateTime"/>.
    /// </summary>
    /// <returns>Current <see cref="DateTime"/>.</returns>
    DateTime GetNow();
}
