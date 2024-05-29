using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono.Internal;

/// <inheritdoc cref="ITimestampProvider" />
public abstract class TimestampProviderBase : ITimestampProvider
{
    /// <inheritdoc />
    public abstract Timestamp GetNow();

    [Pure]
    Timestamp IGenerator<Timestamp>.Generate()
    {
        return GetNow();
    }

    bool IGenerator<Timestamp>.TryGenerate(out Timestamp result)
    {
        result = GetNow();
        return true;
    }

    [Pure]
    object IGenerator.Generate()
    {
        return GetNow();
    }

    bool IGenerator.TryGenerate(out object result)
    {
        result = GetNow();
        return true;
    }
}
