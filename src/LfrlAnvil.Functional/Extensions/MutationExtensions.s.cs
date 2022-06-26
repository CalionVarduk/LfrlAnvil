using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions;

public static class MutationExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Mutation<T> Reduce<T>(this Mutation<Mutation<T>> source)
    {
        return new Mutation<T>( source.OldValue.OldValue, source.Value.Value );
    }
}
