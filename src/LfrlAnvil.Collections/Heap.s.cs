using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections;

/// <summary>
/// Contains helper <see cref="IHeap{T}"/> methods.
/// </summary>
public static class Heap
{
    /// <summary>
    /// Calculates child's parent index.
    /// </summary>
    /// <param name="childIndex">Child index.</param>
    /// <returns>Child's parent index.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int GetParentIndex(int childIndex)
    {
        return (childIndex - 1) >> 1;
    }

    /// <summary>
    /// Calculates parent's left child index.
    /// </summary>
    /// <param name="parentIndex">Parent index.</param>
    /// <returns>Parent's left child index.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int GetLeftChildIndex(int parentIndex)
    {
        return (parentIndex << 1) + 1;
    }

    /// <summary>
    /// Calculates parent's right child index.
    /// </summary>
    /// <param name="parentIndex">Parent index.</param>
    /// <returns>Parent's right child index.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int GetRightChildIndex(int parentIndex)
    {
        return GetLeftChildIndex( parentIndex ) + 1;
    }
}
