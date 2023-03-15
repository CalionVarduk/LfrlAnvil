using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections.Extensions;

public static class GraphDirectionExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static GraphDirection Invert(this GraphDirection direction)
    {
        return (GraphDirection)(((byte)(direction & GraphDirection.In) << 1) | ((byte)(direction & GraphDirection.Out) >> 1));
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static GraphDirection Sanitize(this GraphDirection direction)
    {
        return direction & GraphDirection.Both;
    }
}
