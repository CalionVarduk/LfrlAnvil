using System;

namespace LfrlAnvil;

/// <summary>
/// A lightweight generic container for an optional disposable object.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public readonly struct OptionalDisposable<T> : IDisposable
    where T : IDisposable
{
    /// <summary>
    /// Represents an empty disposable, without an underlying object.
    /// </summary>
    public static readonly OptionalDisposable<T> Empty = new OptionalDisposable<T>();

    internal OptionalDisposable(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Optional underlying disposable object.
    /// </summary>
    public T? Value { get; }

    /// <inheritdoc />
    /// <remarks>Disposes the underlying <see cref="Value"/> if it exists.</remarks>
    public void Dispose()
    {
        Value?.Dispose();
    }
}
