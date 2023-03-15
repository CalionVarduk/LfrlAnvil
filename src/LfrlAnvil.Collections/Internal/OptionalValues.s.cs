using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections.Internal;

internal static class OptionalValues
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool TryGet<TBase, TImpl>(bool exists, TImpl? obj, [MaybeNullWhen( false )] out TBase result)
        where TImpl : TBase
    {
        if ( exists )
        {
            Assume.IsNotNull( obj, nameof( obj ) );
            result = obj;
            return true;
        }

        result = default;
        return false;
    }
}
