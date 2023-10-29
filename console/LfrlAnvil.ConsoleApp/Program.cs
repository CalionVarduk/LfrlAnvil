using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.Reactive;
using LfrlAnvil.Reactive.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Extensions;

{
    var interval = Duration.FromMilliseconds( 1000 / 60.0 );
    var simDuration = Duration.FromSeconds( 30 );

    var timestampProvider = new PreciseTimestampProvider();
    var timer = new ReactiveTimer( timestampProvider, interval, Duration.FromMilliseconds( 1 ), long.MaxValue );
    await RunTimer( timer, () => timer.StartAsync(), simDuration, "Chrono" );
}

async Task RunTimer(IEventSource<WithInterval<long>> timer, Action starter, Duration duration, string name)
{
    Console.WriteLine( $"===== TIMER '{name}' STARTED (for {duration}) (TID: {Thread.CurrentThread.ManagedThreadId})" );
    Console.WriteLine();

    var count = 0;
    long last = -1;
    var minInterval = Duration.MaxValue;
    var maxInterval = Duration.MinValue;
    var intervalMean = 0.0;
    var intervalVarianceBase = 0.0;

    timer.Concurrent()
        .Listen(
            EventListener.Create<WithInterval<long>>(
                e =>
                {
                    ++count;
                    last = e.Event;
                    var prevMean = intervalMean;
                    intervalMean += (e.Interval.Ticks - intervalMean) / count;
                    intervalVarianceBase += (e.Interval.Ticks - prevMean) * (e.Interval.Ticks - intervalMean);
                    minInterval = minInterval.Min( e.Interval );
                    maxInterval = maxInterval.Max( e.Interval );

                    Console.WriteLine(
                        $"[{e.Timestamp.UtcValue:HH:mm:ss.fffffff} ({e.Interval})] {e.Event} (TID: {Thread.CurrentThread.ManagedThreadId})" );
                },
                d => { Console.WriteLine( $"Disposed: {d} (TID: {Thread.CurrentThread.ManagedThreadId})" ); } ) );

    starter();

    await Task.Delay( (int)duration.FullMilliseconds );

    timer.Dispose();

    var variance = intervalVarianceBase / count;
    var stdDev = Duration.FromTicks( (long)Math.Sqrt( variance ) );
    Console.WriteLine(
        $"[INTERVALS] Min: {minInterval}, Max: {maxInterval}, Avg: {Duration.FromTicks( (long)intervalMean )}, StdDev: {stdDev}" );

    Console.WriteLine( $"[CAUGHT EVENTS] {count} out of {last + 1} (skipped {last - count + 1}) ({count / (last + 1.0):P})" );

    Console.WriteLine();
    Console.WriteLine( $"===== TIMER '{name}' FINISHED (TID: {Thread.CurrentThread.ManagedThreadId})" );
    Console.WriteLine();
}
