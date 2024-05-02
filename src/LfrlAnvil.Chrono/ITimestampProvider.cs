using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a provider of <see cref="Timestamp"/> instances.
/// </summary>
public interface ITimestampProvider : IGenerator<Timestamp>
{
    /// <summary>
    /// Returns the current <see cref="Timestamp"/>.
    /// </summary>
    /// <returns>Current <see cref="Timestamp"/>.</returns>
    Timestamp GetNow();
}
