using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.MemoryTests;

public class MemoryExtensionsTests : TestsBase
{
    [Fact]
    public void GetEnumerator_ForReadOnlyMemory_ShouldBeEquivalentToSpanEnumerator()
    {
        ReadOnlyMemory<int> sut = new[] { 1, 2, 3 }.AsMemory();

        var result = new List<int>();
        foreach ( var e in sut )
            result.Add( e );

        result.TestSequence( [ 1, 2, 3 ] ).Go();
    }

    [Fact]
    public void GetEnumerator_ForMemory_ShouldBeEquivalentToSpanEnumerator()
    {
        var sut = new[] { 1, 2, 3 }.AsMemory();

        var result = new List<int>();
        foreach ( var e in sut )
            result.Add( e );

        result.TestSequence( [ 1, 2, 3 ] ).Go();
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectEnumerable_ForArrayBackedMemory()
    {
        var sut = new[] { 1, 2, 3, 4, 5 }.AsMemory( 1, 2 );

        var result = sut.AsEnumerable();

        Assertion.All(
                result.Count.TestEquals( sut.Length ),
                result[0].TestEquals( 2 ),
                result[1].TestEquals( 3 ),
                result.TestSequence( [ 2, 3 ] ) )
            .Go();
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectEnumerable_ForStringBackedMemory()
    {
        var sut = "abcde".AsMemory( 1, 2 );

        var result = sut.AsEnumerable();

        Assertion.All(
                result.Count.TestEquals( sut.Length ),
                result[0].TestEquals( 'b' ),
                result[1].TestEquals( 'c' ),
                result.TestSequence( [ 'b', 'c' ] ) )
            .Go();
    }

    [Fact]
    public void AsEnumerable_ShouldReturnCorrectEnumerable_ForCustom()
    {
        var sut = new Manager( 'b', 'c' ).Memory;

        var result = sut.AsEnumerable();

        Assertion.All(
                result.Count.TestEquals( sut.Length ),
                result[0].TestEquals( 'b' ),
                result[1].TestEquals( 'c' ),
                result.TestSequence( [ 'b', 'c' ] ) )
            .Go();
    }

    private sealed class Manager : MemoryManager<char>
    {
        private readonly string _value;

        public Manager(char first, char second)
        {
            _value = $"{first}{second}";
        }

        public override Span<char> GetSpan()
        {
            ref var first = ref MemoryMarshal.GetReference( _value.AsSpan() );
            return MemoryMarshal.CreateSpan( ref first, 2 );
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            return default;
        }

        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }
    }
}
