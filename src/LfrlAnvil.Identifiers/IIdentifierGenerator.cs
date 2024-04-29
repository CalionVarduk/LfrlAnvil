using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Identifiers;

/// <summary>
/// Represents a generator of <see cref="Identifier"/> instances.
/// </summary>
public interface IIdentifierGenerator : IGenerator<Identifier>
{
    /// <summary>
    /// <see cref="Timestamp"/> of the first possible <see cref="Identifier"/> created by this generator.
    /// </summary>
    Timestamp BaseTimestamp { get; }

    /// <summary>
    /// Extracts a <see cref="Timestamp"/> used to create the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id"><see cref="Identifier"/> to extract <see cref="Timestamp"/> from.</param>
    /// <returns>New <see cref="Timestamp"/> instance.</returns>
    [Pure]
    Timestamp GetTimestamp(Identifier id);
}
