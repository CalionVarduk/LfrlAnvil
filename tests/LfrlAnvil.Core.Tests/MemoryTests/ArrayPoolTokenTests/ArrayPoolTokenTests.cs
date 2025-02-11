using System.Buffers;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Tests.MemoryTests.ArrayPoolTokenTests;

public class ArrayPoolTokenTests : TestsBase
{
    private readonly ArrayPool<int> _pool = ArrayPool<int>.Create();

    [Fact]
    public void Default_ShouldReturnCorrectToken()
    {
        var sut = default( ArrayPoolToken<int> );

        Assertion.All(
                sut.Pool.TestNull(),
                sut.Length.TestEquals( 0 ),
                sut.ClearArray.TestFalse(),
                sut.Source.TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( 0, true )]
    [InlineData( 1, false )]
    [InlineData( 5, false )]
    [InlineData( 11, true )]
    public void RentToken_ShouldReturnCorrectToken(int length, bool clearArray)
    {
        var sut = _pool.RentToken( length, clearArray );

        Assertion.All(
                sut.Pool.TestRefEquals( _pool ),
                sut.Length.TestEquals( length ),
                sut.ClearArray.TestEquals( clearArray ),
                sut.Source.Length.TestGreaterThanOrEqualTo( length ) )
            .Go();
    }

    [Fact]
    public void AsSpan_ShouldReturnEmptySpan_ForDefaultToken()
    {
        var sut = default( ArrayPoolToken<int> );
        var result = sut.AsSpan();
        result.Length.TestEquals( 0 ).Go();
    }

    [Theory]
    [InlineData( 0, true )]
    [InlineData( 1, false )]
    [InlineData( 5, false )]
    [InlineData( 11, true )]
    public void AsSpan_ShouldReturnCorrectSpan(int length, bool clearArray)
    {
        var sut = _pool.RentToken( length, clearArray );
        for ( var i = 0; i < sut.Source.Length; ++i )
            sut.Source[i] = i;

        var result = sut.AsSpan();

        result.TestSequence( Enumerable.Range( 0, length ) ).Go();
    }

    [Fact]
    public void AsMemory_ShouldReturnEmptyMemory_ForDefaultToken()
    {
        var sut = default( ArrayPoolToken<int> );
        var result = sut.AsMemory();
        result.Length.TestEquals( 0 ).Go();
    }

    [Theory]
    [InlineData( 0, true )]
    [InlineData( 1, false )]
    [InlineData( 5, false )]
    [InlineData( 11, true )]
    public void AsMemory_ShouldReturnCorrectMemory(int length, bool clearArray)
    {
        var sut = _pool.RentToken( length, clearArray );
        for ( var i = 0; i < sut.Source.Length; ++i )
            sut.Source[i] = i;

        var result = sut.AsMemory();

        result.TestSequence( Enumerable.Range( 0, length ) ).Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_ForDefaultToken()
    {
        var sut = default( ArrayPoolToken<int> );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void Dispose_ShouldReleaseBufferToPool_WithoutClearingArray()
    {
        var sut = _pool.RentToken( 7, clearArray: false );
        var source = sut.Source;
        for ( var i = 0; i < sut.Source.Length; ++i )
            sut.Source[i] = i;

        sut.Dispose();

        var next = _pool.RentToken( 8 );

        Assertion.All(
                next.Source.TestRefEquals( source ),
                source.TestSequence( Enumerable.Range( 0, sut.Source.Length ) ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldReleaseBufferToPool_WithClearingArray()
    {
        var sut = _pool.RentToken( 7, clearArray: true );
        var source = sut.Source;
        for ( var i = 0; i < sut.Source.Length; ++i )
            sut.Source[i] = i;

        sut.Dispose();

        var next = _pool.RentToken( 8 );

        Assertion.All(
                next.Source.TestRefEquals( source ),
                source.TestSequence( Enumerable.Repeat( 0, sut.Source.Length ) ) )
            .Go();
    }
}
