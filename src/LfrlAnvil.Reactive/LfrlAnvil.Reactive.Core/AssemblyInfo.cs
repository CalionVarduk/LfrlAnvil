using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Reactive.Core.Tests" )]

// TODO:
// external extensions:
// creation:
// Timer<T>(Duration,int count=max) - with start/stop/reset methods
// From(EventHandler) - uses functional Nil
//
// EventListener : EventListener<Nil> - base for events without value, essentially
//
// decorators:
// Delay<T>(Duration)
// Debounce<T>(Duration)
// Sample<T>(Duration)
// Throttle<T>(Duration)
// Audit<T>(Duration)
// Unsafe<T>()
// Maybe<T>() - every React is converted to Some<T>
