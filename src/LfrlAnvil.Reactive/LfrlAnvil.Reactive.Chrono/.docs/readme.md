([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Reactive.Chrono)](https://www.nuget.org/packages/LfrlAnvil.Reactive.Chrono/)

# [LfrlAnvil.Reactive.Chrono](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Reactive/LfrlAnvil.Reactive.Chrono)

This project contains a few functionalities related to timers and schedulers.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Reactive.Chrono/LfrlAnvil.Reactive.Chrono.html).

### Examples

Following is an example of a timer that emits events:
```csharp
// provider of timestamps to use for the timer
ITimestampProvider timestamps = ...;

// creates a new timer that emits events every second
// the timer will do its best to not introduce any time skew,
// however it may lead to some events being skipped,
// when an interval is very short or listeners' reactions take a relatively long time to complete
var timer = new ReactiveTimer( timestamps, interval: Duration.FromSeconds( 1 ) );

// attaches a listener to the timer
timer.Listen( EventListener.Create<WithInterval<long>>( ... ) );

// starts the timer asynchronously
timer.StartAsync();

// signals the timer to stop when it gets the chance
// after fully stopping, the timer can be restarted
timer.Stop();

// disposes the timer
timer.Dispose();
```

Timers can also be used to invoke timer tasks, like so:
```csharp
// represents an example of a timer task identified by the 'foo' key
public class FooTask : TimerTask<string>
{
    public FooTask(Timestamp start)
        : base( key: "foo" )
    {
        // specifies that the next invocation of this task
        // should happen at the start of the next minute
        var datetime = ZonedDateTime.CreateUtc( start );
        datetime = datetime.SetTimeOfDay( datetime.TimeOfDay.TrimToMinute() ) + Duration.FromMinutes( 1 );
        NextInvocationTimestamp = datetime.Timestamp;
    }

    public override async Task InvokeAsync(
        TimerTaskCollection<string> source,
        ReactiveTaskInvocationParams parameters,
        CancellationToken cancellationToken)
    {
        // performs an asynchronous operation associated with this task
        await ...;

        // specifies that the next invocation of this task
        // should happen at the start of the next minute
        NextInvocationTimestamp += Duration.FromMinutes( 1 );
    }
}

// provider of timestamps to use for the timer
ITimestampProvider timestamps = ...;

// creates a new timer that emits events every second
var timer = new ReactiveTimer( timestamps, interval: Duration.FromSeconds( 1 ) );

// registers an instance of a timer task
var tasks = timer.RegisterTasks( new[] { new FooTask( timestamps.GetNow() ) } );

// starts the timer asynchronously
timer.StartAsync();

// disposes timer tasks
tasks.Dispose();
```

Following is an example of a task scheduler:
```csharp
// represents an example of a schedule task identified by the 'foo' key
public class FooTask : ScheduleTask<string>
{
    public FooTask()
        : base( key: "foo" ) { }

    public override async Task InvokeAsync(
        IReactiveScheduler<string> scheduler,
        ReactiveTaskInvocationParams parameters,
        CancellationToken cancellationToken)
    {
        // performs an asynchronous operation associated with this task
        await ...;
    }
}

// provider of timestamps to use for the scheduler
ITimestampProvider timestamps = ...;

// creates a new scheduler with tasks identified by string keys
var scheduler = new ReactiveScheduler<string>( timestamps );

// registers an instance of a schedule task
// that should be invoked 3 times, at:
// 1. 'start + 1 minute'
// 2. 'start + 6 minutes'
// 3. 'start + 11 minutes'
// scheduler also allows to register one-of tasks and infinitely repeating tasks
scheduler.Schedule(
    new FooTask(),
    firstTimestamp: scheduler.StartTimestamp + Duration.FromMinutes( 1 ),
    repetitions: 3,
    interval: Duration.FromMinutes( 5 ) );

// starts the scheduler asynchronously
scheduler.StartAsync();

// disposes the scheduler
scheduler.Dispose();
```
