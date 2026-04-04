using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Chrono.Tests.AsyncTests;

public class PeriodicBatchTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreatePeriodicBatch()
    {
        var delaySource = ValueTaskDelaySource.Start();
        var sut = new Batch( delaySource, Duration.FromSeconds( 1 ), 100 );
        sut.AutoFlushDelay.TestEquals( Duration.FromSeconds( 1 ) ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenAutoFlushDelayIsInvalid(long ticks)
    {
        var delaySource = ValueTaskDelaySource.Start();
        var action = Lambda.Of( () => new Batch( delaySource, Duration.FromTicks( ticks ), 100 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public async Task Ctor_ShouldCreatePeriodicBatch_WhenDelaySourceIsDisposed()
    {
        var delaySource = ValueTaskDelaySource.Start();
        await delaySource.DisposeAsync();
        var sut = new Batch( delaySource, Duration.FromSeconds( 1 ), 100 );
        await Task.Delay( 15 );

        var result = sut.Add( "foo" );

        result.TestFalse().Go();
    }

    [Fact]
    public async Task Dispose_ShouldDisposeUnderlyingTask()
    {
        var delaySource = ValueTaskDelaySource.Start();
        var sut = new Batch( delaySource, Duration.FromTicks( 1 ), 100 );

        await sut.DisposeAsync();
        sut.Add( "foo" );
        await Task.Delay( 15 );

        sut.Events.TestSequence( [ "Disposed" ] ).Go();
    }

    [Fact]
    public async Task DelaySourceDispose_ShouldDisposeBatch()
    {
        var delaySource = ValueTaskDelaySource.Start();
        var sut = new Batch( delaySource, Duration.FromSeconds( 1 ), 100 );

        await Task.Delay( 15 );
        await delaySource.DisposeAsync();
        await Task.Delay( 15 );

        sut.Events.TestSequence( [ "Disposed" ] ).Go();
    }

    [Fact]
    public async Task DelaySourceDispose_ShouldDisposeBatch_AfterAutoFlushWasScheduled()
    {
        var delaySource = ValueTaskDelaySource.Start();
        var sut = new Batch( delaySource, Duration.FromSeconds( 1 ), 100 );

        sut.Add( "foo" );
        await Task.Delay( 15 );
        await delaySource.DisposeAsync();
        await Task.Delay( 15 );

        sut.Events.TestSequence( [ "Process(disposing=True):'foo'", "Disposed" ] ).Go();
    }

    [Fact]
    public async Task Add_ShouldNotTriggerAutoFlush_WhenAutoFlushHappensDueToAutoFlushCountImmediately()
    {
        var completion = new SafeTaskCompletionSource();
        var delaySource = ValueTaskDelaySource.Start();
        var sut = new Batch( delaySource, Duration.FromMinutes( 1 ), 2 );
        sut.OnProcessCallback = () => completion.Complete();

        sut.AddRange( [ "foo", "bar" ] );
        await completion.Task;

        sut.Events.TestSequence( [ "Process(disposing=False):'foo','bar'" ] ).Go();
    }

    [Fact]
    public async Task Add_ShouldCancelAutoFlushTrigger_WhenAutoFlushHappensDueToAutoFlushCountAfterTriggerWasScheduled()
    {
        var completion = new SafeTaskCompletionSource();
        var delaySource = ValueTaskDelaySource.Start();
        var sut = new Batch( delaySource, Duration.FromMinutes( 1 ), 2 );
        sut.OnProcessCallback = () => completion.Complete();

        sut.Add( "foo" );
        await Task.Delay( 15 );
        sut.Add( "bar" );
        await completion.Task;

        sut.Events.TestSequence( [ "Process(disposing=False):'foo','bar'" ] ).Go();
    }

    [Fact]
    public async Task Add_ShouldTriggerAutoFlushAfterDelay()
    {
        var completion = new SafeTaskCompletionSource();
        var delaySource = ValueTaskDelaySource.Start();
        var sut = new Batch( delaySource, Duration.FromMilliseconds( 15 ), 5 );
        sut.OnProcessCallback = () => completion.Complete();

        sut.AddRange( [ "foo", "bar" ] );
        await completion.Task;

        sut.Events.TestSequence( [ "Process(disposing=False):'foo','bar'" ] ).Go();
    }

    private sealed class Batch : PeriodicBatch<string>
    {
        private readonly List<string> _events = new List<string>();

        public Batch(ValueTaskDelaySource delaySource, Duration autoFlushDelay, int autoFlushCount)
            : base( delaySource, autoFlushDelay, autoFlushCount: autoFlushCount ) { }

        public Action? OnProcessCallback { get; set; }
        public IReadOnlyList<string> Events => _events;

        protected override ValueTask<int> ProcessAsync(ReadOnlyMemory<string> items, bool disposing)
        {
            _events.Add( $"Process(disposing={disposing}):{string.Join( ',', items.ToArray().Select( i => $"'{i}'" ) )}" );
            OnProcessCallback?.Invoke();
            return ValueTask.FromResult( items.Length );
        }

        protected override void OnDisposed()
        {
            _events.Add( "Disposed" );
            base.OnDisposed();
        }
    }
}
