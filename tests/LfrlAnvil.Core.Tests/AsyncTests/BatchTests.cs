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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.AsyncTests;

public class BatchTests : TestsBase
{
    [Theory]
    [InlineData( BatchQueueOverflowStrategy.DiscardLast, 1000, int.MaxValue, int.MaxValue )]
    [InlineData( BatchQueueOverflowStrategy.DiscardFirst, 5, 1000, 1000 )]
    [InlineData( BatchQueueOverflowStrategy.Ignore, 50, 20000, 20000 )]
    [InlineData( BatchQueueOverflowStrategy.DiscardLast, 1, 1, 1 )]
    [InlineData( BatchQueueOverflowStrategy.DiscardFirst, 100, 99, 100 )]
    public void Ctor_ShouldReturnCorrectResult(
        BatchQueueOverflowStrategy queueOverflowStrategy,
        int autoFlushCount,
        int queueSizeLimitHint,
        int expectedQueueSizeLimitHint)
    {
        var sut = new Batch( queueOverflowStrategy, autoFlushCount, queueSizeLimitHint );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.QueueOverflowStrategy.Should().Be( queueOverflowStrategy );
            sut.AutoFlushCount.Should().Be( autoFlushCount );
            sut.QueueSizeLimitHint.Should().Be( expectedQueueSizeLimitHint );
            sut.Events.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenAutoFlushCountIsLessThanOne(int value)
    {
        var action = Lambda.Of( () => new Batch( BatchQueueOverflowStrategy.DiscardLast, value, int.MaxValue ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Add_ShouldEnqueueItem()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );

        var result = sut.Add( "e1" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnEnqueued:autoFlushing=False", "e1" ) );
        }
    }

    [Fact]
    public async Task Add_ShouldAutoFlush_WhenEnqueuedElementCountEqualsAutoFlushCount()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 3, int.MaxValue );
        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return Task.CompletedTask;
        };

        sut.Add( "e1" );
        sut.Add( "e2" );

        var result = sut.Add( "e3" );
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=False", "e1" ),
                    new Event( "OnEnqueued:autoFlushing=False", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e3" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ) );
        }
    }

    [Fact]
    public async Task Add_ShouldDoNothing_WhenQueueSizeHintIsExceeded_WithDiscardLastStrategy()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 2, 2 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.Add( "e1" );
        sut.Add( "e2" );
        await taskSource.Task;

        sut.Add( "e3" );
        sut.Add( "e4" );
        var result = sut.Add( "e5" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 2 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=False", "e1" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e2" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=False", "e3" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e4" ) );
        }
    }

    [Fact]
    public async Task Add_ShouldDiscardFirstElements_WhenQueueSizeHintIsExceeded_WithDiscardFirstStrategy()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardFirst, 2, 2 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.Add( "e1" );
        sut.Add( "e2" );
        await taskSource.Task;

        sut.Add( "e3" );
        sut.Add( "e4" );
        var result = sut.Add( "e5" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=False", "e1" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e2" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=False", "e3" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e4" ),
                    new Event( "OnDiscarding:disposing=False", "e3" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e5" ) );
        }
    }

    [Fact]
    public async Task Add_ShouldAddElement_WhenQueueSizeHintIsExceeded_WithIgnoreStrategy()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.Ignore, 2, 2 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.Add( "e1" );
        sut.Add( "e2" );
        await taskSource.Task;

        sut.Add( "e3" );
        sut.Add( "e4" );
        var result = sut.Add( "e5" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 3 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=False", "e1" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e2" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=False", "e3" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e4" ),
                    new Event( "OnEnqueued:autoFlushing=False", "e5" ) );
        }
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenBatchIsDisposed()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );
        sut.Dispose();

        var result = sut.Add( "e1" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnDisposed" ) );
        }
    }

    [Fact]
    public void AddRange_WithSpan_ShouldEnqueueItems()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );

        var result = sut.AddRange( new[] { "e1", "e2" }.AsSpan() );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnEnqueued:autoFlushing=False", "e1", "e2" ) );
        }
    }

    [Fact]
    public async Task AddRange_WithSpan_ShouldAutoFlush_WhenEnqueuedElementCountJustExceededAutoFlushCount()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 3, int.MaxValue );
        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return Task.CompletedTask;
        };

        sut.AddRange( new[] { "e1" }.AsSpan() );

        var result = sut.AddRange( new[] { "e2", "e3" }.AsSpan() );
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=False", "e1" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e2", "e3" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ) );
        }
    }

    [Fact]
    public async Task AddRange_WithSpan_ShouldDoNothing_WhenQueueSizeHintIsExceeded_WithDiscardLastStrategy()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 2, 2 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.AddRange( new[] { "e1", "e2" }.AsSpan() );
        await taskSource.Task;

        sut.AddRange( new[] { "e3", "e4", "e5" }.AsSpan() );
        var result = sut.AddRange( new[] { "e6", "e7" }.AsSpan() );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 3 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e3", "e4", "e5" ) );
        }
    }

    [Fact]
    public async Task AddRange_WithSpan_ShouldDiscardFirstElements_WhenQueueSizeHintIsExceeded_WithDiscardFirstStrategy()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardFirst, 2, 2 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.AddRange( new[] { "e1", "e2" }.AsSpan() );
        await taskSource.Task;

        sut.AddRange( new[] { "e3", "e4", "e5" }.AsSpan() );
        var result = sut.AddRange( new[] { "e6", "e7" }.AsSpan() );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e3", "e4", "e5" ),
                    new Event( "OnDiscarding:disposing=False", "e3", "e4", "e5" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e6", "e7" ) );
        }
    }

    [Fact]
    public async Task AddRange_WithSpan_ShouldAddElements_WhenQueueSizeHintIsExceeded_WithIgnoreStrategy()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.Ignore, 2, 2 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.AddRange( new[] { "e1", "e2" }.AsSpan() );
        await taskSource.Task;

        sut.AddRange( new[] { "e3", "e4", "e5" }.AsSpan() );
        var result = sut.AddRange( new[] { "e6", "e7" }.AsSpan() );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 5 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e3", "e4", "e5" ),
                    new Event( "OnEnqueued:autoFlushing=False", "e6", "e7" ) );
        }
    }

    [Theory]
    [InlineData( BatchQueueOverflowStrategy.DiscardLast )]
    [InlineData( BatchQueueOverflowStrategy.DiscardFirst )]
    [InlineData( BatchQueueOverflowStrategy.Ignore )]
    public void AddRange_WithSpan_ShouldDoNothing_WhenRangeIsEmpty(BatchQueueOverflowStrategy strategy)
    {
        var sut = new Batch( strategy, 10, int.MaxValue );

        var result = sut.AddRange( ReadOnlySpan<string>.Empty );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void AddRange_WithSpan_ShouldDoNothing_WhenBatchIsDisposed()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );
        sut.Dispose();

        var result = sut.AddRange( new[] { "e1" }.AsSpan() );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnDisposed" ) );
        }
    }

    [Fact]
    public void AddRange_ShouldEnqueueItems()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );

        var result = sut.AddRange( new[] { "e1", "e2" }.Where( _ => true ) );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnEnqueued:autoFlushing=False", "e1", "e2" ) );
        }
    }

    [Fact]
    public async Task AddRange_ShouldAutoFlush_WhenEnqueuedElementCountJustExceededAutoFlushCount()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 3, int.MaxValue );
        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return Task.CompletedTask;
        };

        sut.AddRange( new[] { "e1" }.Where( _ => true ) );

        var result = sut.AddRange( new[] { "e2", "e3" }.Where( _ => true ) );
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=False", "e1" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e2", "e3" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ) );
        }
    }

    [Fact]
    public async Task AddRange_ShouldDoNothing_WhenQueueSizeHintIsExceeded_WithDiscardLastStrategy()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 2, 2 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.AddRange( new[] { "e1", "e2" }.Where( _ => true ) );
        await taskSource.Task;

        sut.AddRange( new[] { "e3", "e4", "e5" }.Where( _ => true ) );
        var result = sut.AddRange( new[] { "e6", "e7" }.Where( _ => true ) );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 3 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e3", "e4", "e5" ) );
        }
    }

    [Fact]
    public async Task AddRange_ShouldDiscardFirstElements_WhenQueueSizeHintIsExceeded_WithDiscardFirstStrategy()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardFirst, 2, 2 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.AddRange( new[] { "e1", "e2" }.Where( _ => true ) );
        await taskSource.Task;

        sut.AddRange( new[] { "e3", "e4", "e5" }.Where( _ => true ) );
        var result = sut.AddRange( new[] { "e6", "e7" }.Where( _ => true ) );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e3", "e4", "e5" ),
                    new Event( "OnDiscarding:disposing=False", "e3", "e4" ),
                    new Event( "OnDiscarding:disposing=False", "e5" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e6", "e7" ) );
        }
    }

    [Fact]
    public async Task AddRange_ShouldAddElements_WhenQueueSizeHintIsExceeded_WithIgnoreStrategy()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.Ignore, 2, 2 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.AddRange( new[] { "e1", "e2" }.Where( _ => true ) );
        await taskSource.Task;

        sut.AddRange( new[] { "e3", "e4", "e5" }.Where( _ => true ) );
        var result = sut.AddRange( new[] { "e6", "e7" }.Where( _ => true ) );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 5 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e3", "e4", "e5" ),
                    new Event( "OnEnqueued:autoFlushing=False", "e6", "e7" ) );
        }
    }

    [Theory]
    [InlineData( BatchQueueOverflowStrategy.DiscardLast )]
    [InlineData( BatchQueueOverflowStrategy.DiscardFirst )]
    [InlineData( BatchQueueOverflowStrategy.Ignore )]
    public void AddRange_ShouldDoNothing_WhenRangeIsEmpty(BatchQueueOverflowStrategy strategy)
    {
        var sut = new Batch( strategy, 10, int.MaxValue );

        var result = sut.AddRange( Enumerable.Empty<string>().Where( _ => true ) );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void AddRange_ShouldDoNothing_WhenBatchIsDisposed()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );
        sut.Dispose();

        var result = sut.AddRange( new[] { "e1" }.Where( _ => true ) );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnDisposed" ) );
        }
    }

    [Fact]
    public void AddRange_ShouldAddElements_WhenSourceIsArray()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );

        var result = sut.AddRange( new[] { "e1", "e2", "e3" }.AsEnumerable() );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 3 );
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnEnqueued:autoFlushing=False", "e1", "e2", "e3" ) );
        }
    }

    [Fact]
    public async Task Flush_ShouldDoNothing_WhenBatchIsEmpty()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );

        var result = sut.Flush();
        await Task.Delay( 15 );
        await sut.DisposeAsync();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnDisposed" ) );
        }
    }

    [Fact]
    public async Task Flush_ShouldMarkElementsForProcessing_WhenBatchIsNotEmpty()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );
        sut.AddRange( new[] { "e1", "e2", "e3" }.AsSpan() );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return Task.CompletedTask;
        };

        var result = sut.Flush();
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=False", "e1", "e2", "e3" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ) );
        }
    }

    [Fact]
    public void Flush_ShouldDoNothing_WhenBatchIsDisposed()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );
        sut.Dispose();

        var result = sut.Flush();

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnDisposed" ) );
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenBatchIsEmpty()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );
        sut.Dispose();
        sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnDisposed" ) );
    }

    [Fact]
    public void Dispose_ShouldFlushElements_WhenBatchIsNotEmpty()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );
        sut.AddRange( new[] { "e1", "e2", "e3" }.AsSpan() );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=False", "e1", "e2", "e3" ),
                    new Event( "OnDequeued:disposing=True", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=True", "e1", "e2", "e3" ),
                    new Event( "OnDisposed" ) );
        }
    }

    [Fact]
    public async Task Dispose_ShouldDiscardElementsThatFailedToProcess_WhenBatchIsNotEmpty()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 3, int.MaxValue, 0, 0 );

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return Task.CompletedTask;
        };

        sut.AddRange( new[] { "e1", "e2", "e3", "e4", "e5" }.AsSpan() );
        await taskSource.Task;
        sut.OnProcessAsyncCallback = null;
        await sut.DisposeAsync();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2", "e3", "e4", "e5" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=True", "e1", "e2", "e3" ),
                    new Event( "OnDiscarding:disposing=True", "e1", "e2", "e3" ),
                    new Event( "OnDiscarding:disposing=True", "e4", "e5" ),
                    new Event( "OnDisposed" ) );
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenBatchIsDisposed()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        using ( new AssertionScope() )
        {
            action.Should().NotThrow();
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnDisposed" ) );
        }
    }

    [Fact]
    public async Task FlushingMoreElementsThanAutoFlushCount_ShouldBeHandledCorrectly()
    {
        var processCount = 0;
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 3, int.MaxValue );
        sut.OnProcessAsyncCallback = () =>
        {
            if ( ++processCount == 2 )
                taskSource.SetResult();

            return Task.CompletedTask;
        };

        sut.AddRange( new[] { "e1", "e2", "e3", "e4", "e5" }.AsSpan() );
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2", "e3", "e4", "e5" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ),
                    new Event( "OnDequeued:disposing=False", "e4", "e5" ),
                    new Event( "ProcessAsync:disposing=False", "e4", "e5" ) );
        }
    }

    [Fact]
    public async Task PartialElementProcessingSuccess_ShouldBeHandledCorrectly()
    {
        var processCount = 0;
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 3, int.MaxValue, 1, 2, 1 );
        sut.OnProcessAsyncCallback = () =>
        {
            if ( ++processCount == 4 )
                taskSource.SetResult();

            return Task.CompletedTask;
        };

        sut.AddRange( new[] { "e1", "e2", "e3", "e4", "e5" }.AsSpan() );
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2", "e3", "e4", "e5" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ),
                    new Event( "OnDequeued:disposing=False", "e4" ),
                    new Event( "ProcessAsync:disposing=False", "e2", "e3", "e4" ),
                    new Event( "OnDequeued:disposing=False", "e5" ),
                    new Event( "ProcessAsync:disposing=False", "e4", "e5" ),
                    new Event( "ProcessAsync:disposing=False", "e5" ) );
        }
    }

    [Fact]
    public async Task FailureToProcessAnyElement_ShouldStopCurrentProcessingLoop()
    {
        var processCount = 0;
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 3, int.MaxValue, 1, 0 );
        sut.OnProcessAsyncCallback = () =>
        {
            if ( ++processCount == 2 )
                taskSource.SetResult();

            return Task.CompletedTask;
        };

        sut.AddRange( new[] { "e1", "e2", "e3", "e4", "e5" }.AsSpan() );
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2", "e3", "e4", "e5" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ),
                    new Event( "OnDequeued:disposing=False", "e4" ),
                    new Event( "ProcessAsync:disposing=False", "e2", "e3", "e4" ) );
        }
    }

    [Fact]
    public async Task FlushingAfterFailureToProcessAnyElement_ShouldBeHandledCorrectly()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 3, int.MaxValue, 0 );
        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return Task.CompletedTask;
        };

        sut.AddRange( new[] { "e1", "e2", "e3", "e4", "e5" }.AsSpan() );
        await taskSource.Task;

        var processCount = 0;
        taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        sut.OnProcessAsyncCallback = () =>
        {
            if ( ++processCount == 2 )
                taskSource.SetResult();

            return Task.CompletedTask;
        };

        sut.Flush();
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2", "e3", "e4", "e5" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ),
                    new Event( "OnDequeued:disposing=False", "e4", "e5" ),
                    new Event( "ProcessAsync:disposing=False", "e4", "e5" ) );
        }
    }

    [Fact]
    public async Task DisposeDuringProcessingLoop_ShouldBeHandledCorrectly()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 3, int.MaxValue );
        sut.OnProcessAsyncCallback = () =>
        {
            _ = sut.DisposeAsync().AsTask();
            return Task.CompletedTask;
        };

        sut.OnDisposedCallback = () => taskSource.SetResult();

        sut.AddRange( new[] { "e1", "e2", "e3", "e4", "e5" }.AsSpan() );
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1", "e2", "e3", "e4", "e5" ),
                    new Event( "OnDequeued:disposing=False", "e1", "e2", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e1", "e2", "e3" ),
                    new Event( "OnDequeued:disposing=True", "e4", "e5" ),
                    new Event( "ProcessAsync:disposing=True", "e4", "e5" ),
                    new Event( "OnDisposed" ) );
        }
    }

    [Fact]
    public void OnEnqueued_ShouldNotThrow_WhenExceptionIsThrownByImplementor()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 10, int.MaxValue );
        sut.OnEnqueuedCallback = () => throw new Exception();

        var action = Lambda.Of( () => sut.Add( "e1" ) );

        using ( new AssertionScope() )
        {
            action.Should().NotThrow();
            sut.Count.Should().Be( 1 );
            sut.Events.Should().BeSequentiallyEqualTo( new Event( "OnEnqueued:autoFlushing=False", "e1" ) );
        }
    }

    [Fact]
    public async Task OnDequeued_ShouldNotThrow_WhenExceptionIsThrownByImplementor()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 1, int.MaxValue );
        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return Task.CompletedTask;
        };

        sut.OnDequeuedCallback = () => throw new Exception();

        var action = Lambda.Of( () => sut.Add( "e1" ) );
        action.Should().NotThrow();
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1" ),
                    new Event( "OnDequeued:disposing=False", "e1" ),
                    new Event( "ProcessAsync:disposing=False", "e1" ) );
        }
    }

    [Fact]
    public async Task OnDiscarding_ShouldNotThrow_WhenExceptionIsThrownByImplementor()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var processContinuationSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardFirst, 1, 1 );
        sut.OnDiscardingCallback = () =>
        {
            processContinuationSource.SetResult();
            throw new Exception();
        };

        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return processContinuationSource.Task;
        };

        sut.Add( "e1" );
        await taskSource.Task;

        taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return Task.CompletedTask;
        };

        sut.Add( "e2" );
        var action = Lambda.Of( () => sut.Add( "e3" ) );
        action.Should().NotThrow();
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1" ),
                    new Event( "OnDequeued:disposing=False", "e1" ),
                    new Event( "ProcessAsync:disposing=False", "e1" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e2" ),
                    new Event( "OnDiscarding:disposing=False", "e2" ),
                    new Event( "OnEnqueued:autoFlushing=True", "e3" ),
                    new Event( "OnDequeued:disposing=False", "e3" ),
                    new Event( "ProcessAsync:disposing=False", "e3" ) );
        }
    }

    [Fact]
    public void OnDisposed_ShouldNotThrow_WhenExceptionIsThrownByImplementor()
    {
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 1, int.MaxValue );
        sut.OnDisposedCallback = () => throw new Exception();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }

    [Fact]
    public async Task ProcessAsync_ShouldNotMarkAnyElementAsProcessed_WhenExceptionIsThrownByImplementor()
    {
        var taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        var sut = new Batch( BatchQueueOverflowStrategy.DiscardLast, 1, int.MaxValue );
        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            throw new Exception();
        };

        var action = Lambda.Of( () => sut.Add( "e1" ) );
        action.Should().NotThrow();
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Should()
                .BeSequentiallyEqualTo(
                    new Event( "OnEnqueued:autoFlushing=True", "e1" ),
                    new Event( "OnDequeued:disposing=False", "e1" ),
                    new Event( "ProcessAsync:disposing=False", "e1" ) );
        }

        taskSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        sut.OnProcessAsyncCallback = () =>
        {
            taskSource.SetResult();
            return Task.CompletedTask;
        };

        sut.Flush();
        await taskSource.Task;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Events.Skip( 3 ).Should().BeSequentiallyEqualTo( new Event( "ProcessAsync:disposing=False", "e1" ) );
        }
    }

    private sealed class Batch : Batch<string>
    {
        private readonly List<Event> _events = new List<Event>();
        private readonly Queue<int> _processAsyncResult;

        public Batch(
            BatchQueueOverflowStrategy queueOverflowStrategy,
            int autoFlushCount,
            int queueSizeLimitHint,
            params int[] processAsyncResult)
            : base( queueOverflowStrategy, autoFlushCount, queueSizeLimitHint )
        {
            _processAsyncResult = new Queue<int>( processAsyncResult );
        }

        public Func<Task>? OnProcessAsyncCallback { get; set; }
        public Action? OnEnqueuedCallback { get; set; }
        public Action? OnDequeuedCallback { get; set; }
        public Action? OnDiscardingCallback { get; set; }
        public Action? OnDisposedCallback { get; set; }
        public IReadOnlyList<Event> Events => _events;

        protected override async ValueTask<int> ProcessAsync(ReadOnlyMemory<string> items, bool disposing)
        {
            var elements = new string[items.Length];
            items.CopyTo( elements );
            _events.Add( new Event( $"{nameof( ProcessAsync )}:{nameof( disposing )}={disposing}", elements ) );
            await (OnProcessAsyncCallback?.Invoke() ?? Task.CompletedTask);
            return _processAsyncResult.TryDequeue( out var result ) ? result : items.Length;
        }

        protected override void OnEnqueued(QueueSlimMemory<string> items, bool autoFlushing)
        {
            base.OnEnqueued( items, autoFlushing );
            var elements = new string[items.Length];
            items.CopyTo( elements );
            _events.Add( new Event( $"{nameof( OnEnqueued )}:{nameof( autoFlushing )}={autoFlushing}", elements ) );
            OnEnqueuedCallback?.Invoke();
        }

        protected override void OnDequeued(ReadOnlyMemory<string> items, bool disposing)
        {
            base.OnDequeued( items, disposing );
            var elements = new string[items.Length];
            items.CopyTo( elements );
            _events.Add( new Event( $"{nameof( OnDequeued )}:{nameof( disposing )}={disposing}", elements ) );
            OnDequeuedCallback?.Invoke();
        }

        protected override void OnDiscarding(QueueSlimMemory<string> items, bool disposing)
        {
            base.OnDiscarding( items, disposing );
            var elements = new string[items.Length];
            items.CopyTo( elements );
            _events.Add( new Event( $"{nameof( OnDiscarding )}:{nameof( disposing )}={disposing}", elements ) );
            OnDiscardingCallback?.Invoke();
        }

        protected override void OnDisposed()
        {
            base.OnDisposed();
            _events.Add( new Event( nameof( OnDisposed ), Array.Empty<string>() ) );
            OnDisposedCallback?.Invoke();
        }
    }

    private sealed class Event
    {
        public Event(string type, params string[] elements)
        {
            Type = type;
            Elements = elements;
        }

        public string Type { get; }
        public IReadOnlyList<string> Elements { get; }

        [Pure]
        public override string ToString()
        {
            return $"[{Type}] {string.Join( " > ", Elements )}";
        }

        [Pure]
        public override int GetHashCode()
        {
            return Hash.Default.Add( Type ).AddRange( Elements ).Value;
        }

        [Pure]
        public override bool Equals(object? obj)
        {
            return obj is Event e && Equals( e );
        }

        [Pure]
        public bool Equals(Event? other)
        {
            return other is not null && Type == other.Type && Elements.SequenceEqual( other.Elements );
        }
    }
}
