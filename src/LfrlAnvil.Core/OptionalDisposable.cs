using System;

namespace LfrlAnvil;

public readonly struct OptionalDisposable<T> : IDisposable
    where T : IDisposable
{
    public static readonly OptionalDisposable<T> Empty = new OptionalDisposable<T>();

    internal OptionalDisposable(T value)
    {
        Value = value;
    }

    public T? Value { get; }

    public void Dispose()
    {
        Value?.Dispose();
    }
}
