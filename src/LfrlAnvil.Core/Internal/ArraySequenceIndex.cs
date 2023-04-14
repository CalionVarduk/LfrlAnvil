using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Internal;

internal readonly struct ArraySequenceIndex
{
    internal static readonly ArraySequenceIndex Zero = new ArraySequenceIndex( 0, 0 );
    internal readonly int Segment;
    internal readonly int Element;

    internal ArraySequenceIndex(int segment, int element)
    {
        Assume.IsGreaterThanOrEqualTo( segment, -1, nameof( segment ) );
        Assume.IsGreaterThanOrEqualTo( element, 0, nameof( element ) );
        Segment = segment;
        Element = element;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ArraySequenceIndex MinusOne(int segmentLength)
    {
        Assume.IsGreaterThan( segmentLength, 0, nameof( segmentLength ) );
        return new ArraySequenceIndex( -1, segmentLength - 1 );
    }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Segment )} = {Segment}, {nameof( Element )} = {Element}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ArraySequenceIndex Add(int offset, int segmentLengthLog2)
    {
        Assume.IsGreaterThanOrEqualTo( offset, 0, nameof( offset ) );
        Assume.IsGreaterThanOrEqualTo( segmentLengthLog2, 0, nameof( segmentLengthLog2 ) );
        Assume.IsLessThan( Element, 1 << segmentLengthLog2, nameof( Element ) );

        var nextElement = Element + offset;
        return new ArraySequenceIndex( Segment + (nextElement >> segmentLengthLog2), nextElement & ((1 << segmentLengthLog2) - 1) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ArraySequenceIndex Decrement(int segmentLength)
    {
        Assume.IsGreaterThan( segmentLength, 0, nameof( segmentLength ) );
        Assume.IsLessThan( Element, segmentLength, nameof( Element ) );
        return Element == 0 ? new ArraySequenceIndex( Segment - 1, segmentLength - 1 ) : new ArraySequenceIndex( Segment, Element - 1 );
    }
}
