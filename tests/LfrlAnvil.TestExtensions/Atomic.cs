using System.Diagnostics.Contracts;

namespace LfrlAnvil.TestExtensions;

public sealed class Atomic<T>
{
    private readonly object _lock = new object();
    private T _value;

    public Atomic(T value)
    {
        _value = value;
    }

    public T Value
    {
        get
        {
            lock ( _lock )
                return _value;
        }
        set
        {
            lock ( _lock )
                _value = value;
        }
    }
}

public static class Atomic
{
    [Pure]
    public static Atomic<T> Create<T>(T value)
    {
        return new Atomic<T>( value );
    }
}
