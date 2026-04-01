using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Reactive.Chrono.Tests;

public class ReactiveSchedulerBatchTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldRegisterSchedulerTask()
    {
        var scheduler = new ReactiveScheduler<int>();
        var sut = new Batch( scheduler, 123, Duration.FromSeconds( 1 ), 100 );

        Assertion.All(
                scheduler.TaskKeys.TestSequence( [ 123 ] ),
                sut.AutoFlushDelay.TestEquals( Duration.FromSeconds( 1 ) ),
                sut.Scheduler.TestRefEquals( scheduler ),
                sut.SchedulerKey.TestEquals( 123 ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenAutoFlushDelayIsInvalid(long ticks)
    {
        var scheduler = new ReactiveScheduler<int>();
        var action = Lambda.Of( () => new Batch( scheduler, 123, Duration.FromTicks( ticks ), 100 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowInvalidOperationException_WhenSchedulerIsDisposed()
    {
        var scheduler = new ReactiveScheduler<int>();
        scheduler.Dispose();

        var action = Lambda.Of( () => new Batch( scheduler, 123, Duration.FromSeconds( 1 ), 100 ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowInvalidOperationException_WhenSchedulerKeyIsAlreadyTaken()
    {
        var task = Substitute.For<IScheduleTask<int>>();
        task.Key.Returns( 123 );

        var scheduler = new ReactiveScheduler<int>();
        scheduler.Schedule( task, new Timestamp( 123 ) );

        var action = Lambda.Of( () => new Batch( scheduler, 123, Duration.FromSeconds( 1 ), 100 ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeSchedulerTask()
    {
        var scheduler = new ReactiveScheduler<int>();
        var sut = new Batch( scheduler, 123, Duration.FromSeconds( 1 ), 100 );

        sut.Dispose();

        scheduler.TaskKeys.TestEmpty().Go();
    }

    [Fact]
    public void SchedulerTaskDispose_ShouldDisposeBatch()
    {
        var scheduler = new ReactiveScheduler<int>();
        var sut = new Batch( scheduler, 123, Duration.FromSeconds( 1 ), 100 );

        scheduler.Remove( 123 );

        sut.Flush().TestFalse().Go();
    }

    [Fact]
    public async Task Add_ShouldNotTriggerAutoFlush_WhenAutoFlushHappensDueToAutoFlushCountImmediately()
    {
        var completion = new SafeTaskCompletionSource();
        var scheduler = new ReactiveScheduler<int>();
        var sut = new Batch( scheduler, 123, Duration.FromTicks( 1 ), 2 );
        sut.OnProcessCallback = () => completion.Complete();
        scheduler.Start();

        sut.AddRange( [ "foo", "bar" ] );
        await completion.Task;
        var state = scheduler.TryGetTaskState( 123 );

        Assertion.All(
                state.TestNotNull( s => Assertion.All(
                    "state",
                    s.State.TotalInvocations.TestEquals( 0 ),
                    s.NextTimestamp.TestEquals( new Timestamp( long.MaxValue ) ) ) ),
                sut.Events.TestSequence( [ "Process(disposing=False):'foo','bar'" ] ) )
            .Go();
    }

    [Fact]
    public async Task Add_ShouldCancelAutoFlushTrigger_WhenAutoFlushHappensDueToAutoFlushCountAfterTriggerWasScheduled()
    {
        var completion = new SafeTaskCompletionSource();
        var scheduler = new ReactiveScheduler<int>();
        var sut = new Batch( scheduler, 123, Duration.FromMilliseconds( 15 ), 2 );
        sut.OnProcessCallback = () => completion.Complete();
        scheduler.Start();

        sut.Add( "foo" );
        sut.Add( "bar" );
        await completion.Task;
        var state = scheduler.TryGetTaskState( 123 );

        Assertion.All(
                state.TestNotNull( s => Assertion.All(
                    "state",
                    s.State.TotalInvocations.TestEquals( 0 ),
                    s.NextTimestamp.TestEquals( new Timestamp( long.MaxValue ) ) ) ),
                sut.Events.TestSequence( [ "Process(disposing=False):'foo','bar'" ] ) )
            .Go();
    }

    [Fact]
    public async Task Add_ShouldTriggerAutoFlushAfterDelay()
    {
        var completion = new SafeTaskCompletionSource();
        var scheduler = new ReactiveScheduler<int>();
        var sut = new Batch( scheduler, 123, Duration.FromMilliseconds( 15 ), 5 );
        sut.OnProcessCallback = () => completion.Complete();
        scheduler.Start();

        sut.AddRange( [ "foo", "bar" ] );
        await completion.Task;
        var state = scheduler.TryGetTaskState( 123 );

        Assertion.All(
                state.TestNotNull( s => Assertion.All(
                    "state",
                    s.State.TotalInvocations.TestEquals( 1 ),
                    s.NextTimestamp.TestEquals( new Timestamp( long.MaxValue ) ) ) ),
                sut.Events.TestSequence( [ "Process(disposing=False):'foo','bar'" ] ) )
            .Go();
    }

    [Fact]
    public async Task Add_ShouldTriggerAutoFlushAfterDelay_WithoutDebounce()
    {
        var completion = new SafeTaskCompletionSource();
        var timestamps = new IteratingTimestamps( new Timestamp( 100 ) );
        var scheduler = new ReactiveScheduler<int>( timestamps );
        var sut = new Batch( scheduler, 123, Duration.FromTicks( 123 ), 5 );
        sut.OnProcessCallback = () => completion.Complete();
        scheduler.Start();

        sut.Add( "foo" );
        sut.Add( "bar" );
        await completion.Task;
        var state = scheduler.TryGetTaskState( 123 );

        Assertion.All(
                state.TestNotNull( s => Assertion.All(
                    "state",
                    s.State.TotalInvocations.TestEquals( 1 ),
                    s.State.LastInvocationTimestamp.TestEquals( new Timestamp( 226 ) ),
                    s.NextTimestamp.TestEquals( new Timestamp( long.MaxValue ) ) ) ),
                sut.Events.TestSequence( [ "Process(disposing=False):'foo','bar'" ] ) )
            .Go();
    }

    private sealed class Batch : ReactiveSchedulerBatch<int, string>
    {
        private readonly List<string> _events = new List<string>();

        public Batch(ReactiveScheduler<int> scheduler, int key, Duration autoFlushDelay, int autoFlushCount)
            : base( scheduler, key, autoFlushDelay, autoFlushCount: autoFlushCount ) { }

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

    private sealed class IteratingTimestamps : TimestampProviderBase
    {
        private readonly Timestamp _start;
        private Duration _offset = Duration.Zero;

        internal IteratingTimestamps(Timestamp start)
        {
            _start = start;
        }

        [Pure]
        public override Timestamp GetNow()
        {
            using ( ExclusiveLock.Enter( this ) )
            {
                var result = _start + _offset;
                _offset = _offset.AddTicks( 1 );
                return result;
            }
        }
    }
}
