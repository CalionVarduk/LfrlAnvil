using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

public sealed class Ref<T> : IReadOnlyRef<T>
{
    public Ref(T value)
    {
        Value = value;
    }

    public T Value { get; set; }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Ref )}({Value})";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator T(Ref<T> obj)
    {
        return obj.Value;
    }

    [Pure]
    public static explicit operator Ref<T>(T value)
    {
        return new Ref<T>( value );
    }
}
