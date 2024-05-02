using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections.Extensions;

/// <summary>
/// Contains <see cref="GraphDirection"/> extension methods.
/// </summary>
public static class GraphDirectionExtensions
{
    /// <summary>
    /// Inverts the provided <paramref name="direction"/>.
    /// Returns <see cref="GraphDirection.Out"/> for <see cref="GraphDirection.In"/>,
    /// <see cref="GraphDirection.In"/> for <see cref="GraphDirection.Out"/>
    /// and <see cref="GraphDirection.Both"/> for <see cref="GraphDirection.Both"/>.
    /// </summary>
    /// <param name="direction">Direction to invert.</param>
    /// <returns>Inverted <paramref name="direction"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static GraphDirection Invert(this GraphDirection direction)
    {
        return ( GraphDirection )((( byte )(direction & GraphDirection.In) << 1) | (( byte )(direction & GraphDirection.Out) >> 1));
    }

    /// <summary>
    /// Sanitizes the provided <paramref name="direction"/> by computing bitwise and with <see cref="GraphDirection.Both"/>.
    /// </summary>
    /// <param name="direction">Direction to sanitize.</param>
    /// <returns>Sanitized <paramref name="direction"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static GraphDirection Sanitize(this GraphDirection direction)
    {
        return direction & GraphDirection.Both;
    }
}
