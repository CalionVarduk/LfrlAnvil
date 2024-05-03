using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// A lightweight generic default container for an injected member.
/// </summary>
/// <typeparam name="T">Member type.</typeparam>
public readonly struct Injected<T>
{
    /// <summary>
    /// Creates a new <see cref="Injected{T}"/> instance.
    /// </summary>
    /// <param name="instance">Underlying value.</param>
    public Injected(T instance)
    {
        Instance = instance;
    }

    /// <summary>
    /// Underlying value.
    /// </summary>
    public T Instance { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="Injected{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Injected )}({Instance})";
    }
}
