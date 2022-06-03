using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Reactive.Tests" )]

// TODO:
// Add static EventSource class with creation methods:
// Merge<T>(IEnumerable<IEventStream<T>>,int concurrent)
// Concat<T>(IEnumerable<IEventStream<T>>)
// Switch<T>(IEnumerable<IEventStream<T>>)
// Exhaust<T>(IEnumerable<IEventStream<T>>)
//
// external extensions:
// creation:
// Timer<T>(Duration,int count=max) - with start/stop/reset methods
// From(EventHandler) - uses functional Nil
//
// EventListener : EventListener<Nil> - base for events without value, essentially
//
// decorators:
// Delay<T>(Duration)
// WithTimestamp<T>() - attaches a timestamp to the event
// WithInterval<T>() - attaches both a timestamp & a difference between the previous & the current event's timestamps
// Debounce<T>(Duration)
// Sample<T>(Duration)
// Throttle<T>(Duration)
// Audit<T>(Duration)
// Unsafe<T>()
// Maybe<T>() - every React is converted to Some<T>
