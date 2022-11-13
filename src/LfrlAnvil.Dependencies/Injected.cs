using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public readonly struct Injected<T>
{
    public Injected(T instance)
    {
        Instance = instance;
    }

    public T Instance { get; }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Injected )}({Instance})";
    }
}
