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

using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.QueueSlimTests;

public class QueueSlimMemoryTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnCorrectResult()
    {
        var sut = QueueSlimMemory<string>.Empty;

        using ( new AssertionScope() )
        {
            sut.First.ToArray().Should().BeEmpty();
            sut.Second.ToArray().Should().BeEmpty();
            sut.Length.Should().Be( 0 );
        }
    }

    [Fact]
    public void Slice_WithLength_ShouldReturnCorrectResult()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        var sut = queue.AsMemory();

        var result = sut.Slice( 1, 2 );

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeSequentiallyEqualTo( "x2", "x3" );
            result.Second.ToArray().Should().BeEmpty();
            result.Length.Should().Be( 2 );
        }
    }

    [Fact]
    public void Slice_WithLength_ShouldReturnCorrectResult_WhenQueueIsWrapped()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.Dequeue();
        queue.Enqueue( "x5" );
        var sut = queue.AsMemory();

        var result = sut.Slice( 1, 2 );

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeSequentiallyEqualTo( "x3", "x4" );
            result.Second.ToArray().Should().BeEmpty();
            result.Length.Should().Be( 2 );
        }
    }

    [Fact]
    public void Slice_WithLength_ShouldReturnCorrectResult_Wrapped()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 2 );
        queue.EnqueueRange( new[] { "x5", "x6" } );
        var sut = queue.AsMemory();

        var result = sut.Slice( 1, 3 );

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeSequentiallyEqualTo( "x4" );
            result.Second.ToArray().Should().BeSequentiallyEqualTo( "x5", "x6" );
            result.Length.Should().Be( 3 );
        }
    }

    [Fact]
    public void Slice_WithLength_ShouldReturnCorrectResult_FromSecondRange()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 3 );
        queue.EnqueueRange( new[] { "x5", "x6", "x7" } );
        var sut = queue.AsMemory();

        var result = sut.Slice( 2, 2 );

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeSequentiallyEqualTo( "x6", "x7" );
            result.Second.ToArray().Should().BeEmpty();
            result.Length.Should().Be( 2 );
        }
    }

    [Fact]
    public void Slice_ShouldReturnCorrectResult()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 2 );
        queue.EnqueueRange( new[] { "x5", "x6" } );
        var sut = queue.AsMemory();

        var result = sut.Slice( 1 );

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeSequentiallyEqualTo( "x4" );
            result.Second.ToArray().Should().BeSequentiallyEqualTo( "x5", "x6" );
            result.Length.Should().Be( 3 );
        }
    }

    [Fact]
    public void CopyTo_ShouldDoNothing_WhenMemoryIsEmpty()
    {
        var sut = QueueSlimMemory<string>.Empty;
        var target = new[] { "foo" };

        sut.CopyTo( target );

        target.Should().BeSequentiallyEqualTo( "foo" );
    }

    [Fact]
    public void CopyTo_ShouldCopyFirstAndSecondToBuffer()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.Dequeue();
        queue.Enqueue( "x5" );
        var sut = queue.AsMemory();
        var target = new[] { "a", "b", "c", "d", "e" };

        sut.CopyTo( target );

        target.Should().BeSequentiallyEqualTo( "x2", "x3", "x4", "x5", "e" );
    }

    [Theory]
    [InlineData( 0, "x2" )]
    [InlineData( 1, "x3" )]
    [InlineData( 2, "x4" )]
    [InlineData( 3, "x5" )]
    public void Indexer_ShouldReturnCorrectIte(int index, string expected)
    {
        var queue = QueueSlim<string>.Create();
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.Dequeue();
        queue.Enqueue( "x5" );
        var sut = queue.AsMemory();

        var result = sut[index];

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var queue = QueueSlim<string>.Create();
        queue.EnqueueRange( new[] { "x1", "x2", "x3" } );
        queue.Dequeue();
        var sut = queue.AsMemory();

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmpty_WhenQueueIsEmpty()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 ).AsMemory();

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        var sut = queue.AsMemory();

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult_AfterDequeue()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3" } );
        queue.Dequeue();
        var sut = queue.AsMemory();

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x2", "x3" );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult_Wrapped()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 2 );
        queue.Enqueue( "x5" );
        var sut = queue.AsMemory();

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x3", "x4", "x5" );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult_WrappedAtFullCapacity()
    {
        var queue = QueueSlim<string>.Create( minCapacity: 4 );
        queue.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        queue.DequeueRange( 3 );
        queue.EnqueueRange( new[] { "x5", "x6", "x7" } );
        var sut = queue.AsMemory();

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x4", "x5", "x6", "x7" );
    }
}
