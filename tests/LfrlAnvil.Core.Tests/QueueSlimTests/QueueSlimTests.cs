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

public class QueueSlimTests : TestsBase
{
    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 0 )]
    [InlineData( 1, 4 )]
    [InlineData( 3, 4 )]
    [InlineData( 4, 4 )]
    [InlineData( 5, 8 )]
    [InlineData( 17, 32 )]
    public void Create_ShouldReturnEmptyQueue(int minCapacity, int expectedCapacity)
    {
        var sut = QueueSlim<string>.Create( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
            sut.Capacity.Should().Be( expectedCapacity );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddItemToEmptyQueue()
    {
        var sut = QueueSlim<string>.Create();

        sut.Enqueue( "foo" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "foo" );
            sut.Last().Should().Be( "foo" );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddItemsSequentiallyToEmptyQueue_BelowCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.Enqueue( "x1" );
        sut.Enqueue( "x2" );
        sut.Enqueue( "x3" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x3" );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddItemsSequentiallyToEmptyQueue_UpToCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.Enqueue( "x1" );
        sut.Enqueue( "x2" );
        sut.Enqueue( "x3" );
        sut.Enqueue( "x4" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x4" );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddItemsSequentiallyToEmptyQueue_ExceedingCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.Enqueue( "x1" );
        sut.Enqueue( "x2" );
        sut.Enqueue( "x3" );
        sut.Enqueue( "x4" );
        sut.Enqueue( "x5" );
        sut.Enqueue( "x6" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x6" );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddItemToQueue_AfterDequeue()
    {
        var sut = QueueSlim<string>.Create();
        sut.Enqueue( "x1" );
        sut.Enqueue( "x2" );
        sut.Dequeue();

        sut.Enqueue( "x3" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x3" );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddItemToQueue_AfterDequeue_Wrapped()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.Enqueue( "x1" );
        sut.Enqueue( "x2" );
        sut.Enqueue( "x3" );
        sut.Enqueue( "x4" );
        sut.Dequeue();
        sut.Dequeue();

        sut.Enqueue( "x5" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x3" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddItemToQueue_AfterDequeue_AtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.Enqueue( "x1" );
        sut.Enqueue( "x2" );
        sut.Enqueue( "x3" );
        sut.Enqueue( "x4" );
        sut.Dequeue();

        sut.Enqueue( "x5" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddItemToQueue_AfterDequeue_WrappedAndAtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.Enqueue( "x1" );
        sut.Enqueue( "x2" );
        sut.Enqueue( "x3" );
        sut.Enqueue( "x4" );
        sut.Dequeue();
        sut.Dequeue();
        sut.Enqueue( "x5" );

        sut.Enqueue( "x6" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x3" );
            sut.Last().Should().Be( "x6" );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddItemsToQueue_AfterDequeue_ExceedingCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.Enqueue( "x1" );
        sut.Enqueue( "x2" );
        sut.Enqueue( "x3" );
        sut.Enqueue( "x4" );
        sut.Dequeue();
        sut.Enqueue( "x5" );

        sut.Enqueue( "x6" );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 5 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x6" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldDoNothing_WhenItemsAreEmpty()
    {
        var sut = QueueSlim<string>.Create();

        sut.EnqueueRange( ReadOnlySpan<string>.Empty );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsToEmptyQueue()
    {
        var sut = QueueSlim<string>.Create();

        sut.EnqueueRange( new[] { "x1", "x2" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x2" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsSequentiallyToEmptyQueue_BelowCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1" } );
        sut.EnqueueRange( new[] { "x2", "x3" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x3" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsSequentiallyToEmptyQueue_UpToCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1" } );
        sut.EnqueueRange( new[] { "x2", "x3", "x4" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x4" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsSequentiallyToEmptyQueue_ExceedingCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1" } );
        sut.EnqueueRange( new[] { "x2", "x3", "x4", "x5", "x6" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x6" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsToQueue_AfterDequeue()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2" } );
        sut.Dequeue();

        sut.EnqueueRange( new[] { "x3", "x4" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x4" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsToQueue_AfterDequeue_Wrapped()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Dequeue();

        sut.EnqueueRange( new[] { "x5" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x3" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsToQueue_AfterDequeue_AtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3" } );
        sut.Dequeue();

        sut.EnqueueRange( new[] { "x4", "x5" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsToQueue_AfterDequeue_WrappedAndAtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Dequeue();
        sut.Dequeue();
        sut.EnqueueRange( new[] { "x5" } );

        sut.EnqueueRange( new[] { "x6", "x7" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x4" );
            sut.Last().Should().Be( "x7" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsToQueue_AfterDequeue_ExceedingCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Dequeue();
        sut.EnqueueRange( new[] { "x5" } );

        sut.EnqueueRange( new[] { "x6", "x7" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 5 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x3" );
            sut.Last().Should().Be( "x7" );
        }
    }

    [Fact]
    public void EnqueueRange_ShouldAddItemsToQueue_AfterDequeue_AtFullCapacityAndExceedingCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.EnqueueRange( new[] { "x5" } );

        sut.EnqueueRange( new[] { "x6", "x7" } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 6 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x7" );
        }
    }

    [Fact]
    public void Dequeue_ShouldDoNothing_WhenQueueIsEmpty()
    {
        var sut = QueueSlim<string>.Create();

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Dequeue_ShouldRemoveOnlyItemFromQueue()
    {
        var sut = QueueSlim<string>.Create();
        sut.Enqueue( "foo" );

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Dequeue_ShouldRemoveFirstItemFromQueue()
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3" } );

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x3" );
        }
    }

    [Fact]
    public void Dequeue_ShouldRemoveFirstItemFromQueue_Wrapped()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Dequeue();
        sut.Enqueue( "x5" );

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x4" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void Dequeue_ShouldRemoveFirstItemFromQueue_WrappedAtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Enqueue( "x5" );

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x3" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void Dequeue_ShouldRemoveFirstItemFromQueue_Wrapped_WhenRemovalUnwraps()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Dequeue();
        sut.Dequeue();
        sut.Enqueue( "x5" );

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x5" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void DequeueRange_ShouldDoNothing_WhenQueueIsEmpty()
    {
        var sut = QueueSlim<string>.Create();

        var result = sut.DequeueRange( 1 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void DequeueRange_ShouldDoNothing_WhenCountIsLessThanOrEqualToZero(int count)
    {
        var sut = QueueSlim<string>.Create();
        sut.Enqueue( "foo" );

        var result = sut.DequeueRange( count );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void DequeueRange_ShouldRemoveAllItemsFromQueue(int count)
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3" } );

        var result = sut.DequeueRange( count );

        using ( new AssertionScope() )
        {
            result.Should().Be( 3 );
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void DequeueRange_ShouldRemoveFirstItemsFromQueue()
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3" } );

        var result = sut.DequeueRange( 2 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x3" );
            sut.Last().Should().Be( "x3" );
        }
    }

    [Fact]
    public void DequeueRange_ShouldRemoveFirstItemsFromQueue_Wrapped()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 8 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8" } );
        sut.DequeueRange( 2 );
        sut.Enqueue( "x9" );

        var result = sut.DequeueRange( 5 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 5 );
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x8" );
            sut.Last().Should().Be( "x9" );
        }
    }

    [Fact]
    public void DequeueRange_ShouldRemoveFirstItemsFromQueue_WrappedAtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 2 );
        sut.EnqueueRange( new[] { "x5", "x6" } );

        var result = sut.DequeueRange( 2 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x5" );
            sut.Last().Should().Be( "x6" );
        }
    }

    [Fact]
    public void DequeueRange_ShouldRemoveFirstItemsFromQueue_Wrapped_WhenRemovalUnwraps()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 2 );
        sut.Enqueue( "x5" );

        var result = sut.DequeueRange( 2 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x5" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void DequeueRange_ShouldRemoveFirstItemsFromQueue_Wrapped_WhenRemovalUnwrapsAndRemovesFromWrappedRange()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 8 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8" } );
        sut.DequeueRange( 4 );
        sut.EnqueueRange( new[] { "x9", "x10", "x11" } );

        var result = sut.DequeueRange( 5 );

        using ( new AssertionScope() )
        {
            result.Should().Be( 5 );
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x10" );
            sut.Last().Should().Be( "x11" );
        }
    }

    [Theory]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void DequeueRange_ShouldRemoveAllItemsFromQueue_Wrapped(int count)
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 2 );
        sut.EnqueueRange( new[] { "x5" } );

        var result = sut.DequeueRange( count );

        using ( new AssertionScope() )
        {
            result.Should().Be( 3 );
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Clear_ShouldDoNothing_WhenQueueIsEmpty()
    {
        var sut = QueueSlim<string>.Create();

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3" } );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_AtFullCapacity()
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_Wrapped()
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 2 );
        sut.Enqueue( "x5" );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems_WrappedAndAtFullCapacity()
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Enqueue( "x5" );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void ResetCapacity_ShouldSetCapacityToZero_WhenQueueIsEmptyAndMinCapacityIsLessThanOne(int minCapacity)
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 0 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenQueueIsEmptyAndNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    public void ResetCapacity_ShouldDoNothing_WhenNewCapacityDoesNotChange(int minCapacity)
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.Enqueue( "foo" );

        sut.ResetCapacity( minCapacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "foo" );
            sut.Last().Should().Be( "foo" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenQueueIsEmptyAndNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 16 );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 16 );
        sut.EnqueueRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x2" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AfterDequeue()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 16 );
        sut.EnqueueRange( new[] { "x1", "x2" } );
        sut.Dequeue();
        sut.Enqueue( "x3" );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x3" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldReduceCapacity_WhenNewCapacityIsLessThanCurrentCapacity_AtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 16 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 4 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 4 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x4" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenQueueIsEmptyAndNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeTrue();
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2" } );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x2" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_AfterDequeue()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2" } );
        sut.Dequeue();
        sut.Enqueue( "x3" );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x3" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_AtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x1" );
            sut.Last().Should().Be( "x4" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_Wrapped()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 2 );
        sut.Enqueue( "x5" );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x3" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void ResetCapacity_ShouldIncreaseCapacity_WhenNewCapacityIsGreaterThanCurrentCapacity_WrappedAndAtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Enqueue( "x5" );

        sut.ResetCapacity( minCapacity: 8 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 4 );
            sut.Capacity.Should().Be( 8 );
            sut.IsEmpty.Should().BeFalse();
            sut.First().Should().Be( "x2" );
            sut.Last().Should().Be( "x5" );
        }
    }

    [Fact]
    public void AsMemory_ShouldReturnEmpty_WhenQueueIsEmpty()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );

        var result = sut.AsMemory();

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeEmpty();
            result.Second.ToArray().Should().BeEmpty();
            result.Length.Should().Be( 0 );
        }
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectResult()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut.AsMemory();

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
            result.Second.ToArray().Should().BeEmpty();
            result.Length.Should().Be( 4 );
        }
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectResult_AfterDequeue()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3" } );
        sut.Dequeue();

        var result = sut.AsMemory();

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeSequentiallyEqualTo( "x2", "x3" );
            result.Second.ToArray().Should().BeEmpty();
            result.Length.Should().Be( 2 );
        }
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectResult_Wrapped()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 2 );
        sut.Enqueue( "x5" );

        var result = sut.AsMemory();

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeSequentiallyEqualTo( "x3", "x4" );
            result.Second.ToArray().Should().BeSequentiallyEqualTo( "x5" );
            result.Length.Should().Be( 3 );
        }
    }

    [Fact]
    public void AsMemory_ShouldReturnCorrectResult_WrappedAtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 3 );
        sut.EnqueueRange( new[] { "x5", "x6", "x7" } );

        var result = sut.AsMemory();

        using ( new AssertionScope() )
        {
            result.First.ToArray().Should().BeSequentiallyEqualTo( "x4" );
            result.Second.ToArray().Should().BeSequentiallyEqualTo( "x5", "x6", "x7" );
            result.Length.Should().Be( 4 );
        }
    }

    [Theory]
    [InlineData( 0, "x1" )]
    [InlineData( 1, "x2" )]
    [InlineData( 2, "x3" )]
    [InlineData( 3, "x4" )]
    public void Indexer_ShouldReturnCorrectItem(int index, string expected)
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = sut[index];

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, "x3" )]
    [InlineData( 1, "x4" )]
    [InlineData( 2, "x5" )]
    public void Indexer_ShouldReturnCorrectItem_Wrapped(int index, string expected)
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 2 );
        sut.Enqueue( "x5" );

        var result = sut[index];

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, "x2" )]
    [InlineData( 1, "x3" )]
    [InlineData( 2, "x4" )]
    [InlineData( 3, "x5" )]
    public void Indexer_ShouldReturnCorrectItem_WrappedAtFullCapacity(int index, string expected)
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Enqueue( "x5" );

        var result = sut[index];

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange(int index)
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3" } );
        sut.Dequeue();

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange_Wrapped(int index)
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 3 );
        sut.Enqueue( "x5" );

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 4 )]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfRange_WrappedAtFullCapacitu(int index)
    {
        var sut = QueueSlim<string>.Create();
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.Dequeue();
        sut.Enqueue( "x5" );

        var action = Lambda.Of( () => sut[index] );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Indexer_ShouldThrowArgumentOutOfRangeException_WhenQueueIsEmpty()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        var action = Lambda.Of( () => sut[0] );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnEmpty_WhenQueueIsEmpty()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x1", "x2", "x3", "x4" );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult_AfterDequeue()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3" } );
        sut.Dequeue();

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x2", "x3" );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult_Wrapped()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 2 );
        sut.Enqueue( "x5" );

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x3", "x4", "x5" );
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult_WrappedAtFullCapacity()
    {
        var sut = QueueSlim<string>.Create( minCapacity: 4 );
        sut.EnqueueRange( new[] { "x1", "x2", "x3", "x4" } );
        sut.DequeueRange( 3 );
        sut.EnqueueRange( new[] { "x5", "x6", "x7" } );

        var result = new List<string>();
        foreach ( var e in sut )
            result.Add( e );

        result.Should().BeSequentiallyEqualTo( "x4", "x5", "x6", "x7" );
    }
}
