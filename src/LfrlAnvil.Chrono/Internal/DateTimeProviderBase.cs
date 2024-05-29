using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono.Internal;

/// <inheritdoc cref="IDateTimeProvider" />
public abstract class DateTimeProviderBase : IDateTimeProvider
{
    /// <summary>
    /// Creates a new <see cref="DateTimeProviderBase"/> instance.
    /// </summary>
    /// <param name="kind">Specifies the resulting <see cref="DateTimeKind"/> of created instances.</param>
    protected DateTimeProviderBase(DateTimeKind kind)
    {
        Kind = kind;
    }

    /// <inheritdoc />
    public DateTimeKind Kind { get; }

    /// <inheritdoc />
    public abstract DateTime GetNow();

    [Pure]
    DateTime IGenerator<DateTime>.Generate()
    {
        return GetNow();
    }

    bool IGenerator<DateTime>.TryGenerate(out DateTime result)
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
