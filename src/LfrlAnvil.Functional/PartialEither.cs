using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional;

/// <summary>
/// An intermediate object used for creating <see cref="Either{T1,T2}"/> instances.
/// </summary>
/// <typeparam name="T1">Value type.</typeparam>
public readonly struct PartialEither<T1>
{
    /// <summary>
    /// Underlying value.
    /// </summary>
    public readonly T1 Value;

    internal PartialEither(T1 value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance with the underlying <see cref="Value"/> being the first value.
    /// </summary>
    /// <typeparam name="T2">Second value type.</typeparam>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T1, T2> WithSecond<T2>()
    {
        return new Either<T1, T2>( Value );
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance with the underlying <see cref="Value"/> being the second value.
    /// </summary>
    /// <typeparam name="T2">First value type.</typeparam>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T2, T1> WithFirst<T2>()
    {
        return new Either<T2, T1>( Value );
    }
}
