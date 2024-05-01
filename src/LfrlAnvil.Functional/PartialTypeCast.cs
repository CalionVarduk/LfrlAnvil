using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional;

/// <summary>
/// An intermediate object used for creating <see cref="TypeCast{TSource,TDestination}"/> instances.
/// </summary>
/// <typeparam name="TSource">Source object type.</typeparam>
public readonly struct PartialTypeCast<TSource>
{
    /// <summary>
    /// Underlying source object.
    /// </summary>
    public readonly TSource Value;

    internal PartialTypeCast(TSource value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="TypeCast{TSource,TDestination}"/> instance by casting the source <see cref="Value"/>
    /// to the provided <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns>New <see cref="TypeCast{TSource,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TypeCast<TSource, TDestination> To<TDestination>()
    {
        return new TypeCast<TSource, TDestination>( Value );
    }
}
