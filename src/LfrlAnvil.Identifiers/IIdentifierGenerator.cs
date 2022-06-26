using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Identifiers;

public interface IIdentifierGenerator : IGenerator<Identifier>
{
    Timestamp BaseTimestamp { get; }

    [Pure]
    Timestamp GetTimestamp(Identifier id);
}
