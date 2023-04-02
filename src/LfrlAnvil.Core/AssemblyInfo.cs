using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Core.Tests" )]

// TODO:
// project idea: Reactive.Scheduling
// - combines Reactive.Chrono & Reactive.Queues
// - contains sth like IReactiveTimerProvider, caches timers by some sort of key
// - contains TimedCache, which runs on a timer & uses an underlying reorderable queue
// ^ each entry can have a different lifetime
// - contains schedulers, that run on a timer & use an underlying queue
// ^ can they delegate event handling to a different thread?
