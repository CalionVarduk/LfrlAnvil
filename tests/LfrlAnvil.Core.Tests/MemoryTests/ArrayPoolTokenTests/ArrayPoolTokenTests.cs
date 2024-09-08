// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Buffers;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.Memory;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.MemoryTests.ArrayPoolTokenTests;

public class ArrayPoolTokenTests : TestsBase
{
    private readonly ArrayPool<int> _pool = ArrayPool<int>.Create();

    [Fact]
    public void Default_ShouldReturnCorrectToken()
    {
        var sut = default( ArrayPoolToken<int> );

        using ( new AssertionScope() )
        {
            sut.Pool.Should().BeNull();
            sut.Length.Should().Be( 0 );
            sut.ClearArray.Should().BeFalse();
            sut.Source.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( 0, true )]
    [InlineData( 1, false )]
    [InlineData( 5, false )]
    [InlineData( 11, true )]
    public void RentToken_ShouldReturnCorrectToken(int length, bool clearArray)
    {
        var sut = _pool.RentToken( length, clearArray );

        using ( new AssertionScope() )
        {
            sut.Pool.Should().BeSameAs( _pool );
            sut.Length.Should().Be( length );
            sut.ClearArray.Should().Be( clearArray );
            sut.Source.Should().HaveCountGreaterOrEqualTo( length );
        }
    }

    [Fact]
    public void AsSpan_ShouldReturnEmptySpan_ForDefaultToken()
    {
        var sut = default( ArrayPoolToken<int> );
        var result = sut.AsSpan();
        result.Length.Should().Be( 0 );
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

        result.ToArray().Should().BeSequentiallyEqualTo( Enumerable.Range( 0, length ) );
    }

    [Fact]
    public void AsMemory_ShouldReturnEmptyMemory_ForDefaultToken()
    {
        var sut = default( ArrayPoolToken<int> );
        var result = sut.AsMemory();
        result.Length.Should().Be( 0 );
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

        result.ToArray().Should().BeSequentiallyEqualTo( Enumerable.Range( 0, length ) );
    }

    [Fact]
    public void Dispose_ShouldDoNothing_ForDefaultToken()
    {
        var sut = default( ArrayPoolToken<int> );
        var action = Lambda.Of( () => sut.Dispose() );
        action.Should().NotThrow();
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

        using ( new AssertionScope() )
        {
            next.Source.Should().BeSameAs( source );
            source.Should().BeSequentiallyEqualTo( Enumerable.Range( 0, sut.Source.Length ) );
        }
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

        using ( new AssertionScope() )
        {
            next.Source.Should().BeSameAs( source );
            source.Should().BeSequentiallyEqualTo( Enumerable.Repeat( 0, sut.Source.Length ) );
        }
    }
}
