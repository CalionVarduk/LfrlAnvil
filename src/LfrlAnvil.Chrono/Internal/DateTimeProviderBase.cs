using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono.Internal;

public abstract class DateTimeProviderBase : IDateTimeProvider
{
    protected DateTimeProviderBase(DateTimeKind kind)
    {
        Kind = kind;
    }

    public DateTimeKind Kind { get; }

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
