using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Reactive.Core.Tests" )]

// TODO:
// external extensions:
// creation:
// From(EventHandler) - uses functional Nil
//
// EventListener : EventListener<Nil> - base for events without value, essentially
//
// decorators:
// Unsafe<T>()
// Maybe<T>() - every React is converted to Some<T>
