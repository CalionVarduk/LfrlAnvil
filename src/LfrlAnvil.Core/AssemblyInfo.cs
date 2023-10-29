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
//
// project idea: Terminal
// - contains extensions to System.Console
// - write colored fore/back-ground (with temp IDisposable swapper)
// - write table
// - prompt, switch etc. for user interaction
//
// project idea: Diagnostics
// - move most of Core.Diagnostics (except for stopwatch-related stuff & memory size) to that project
// - separate Benchmark into Macro & Micro benchmarks, macro stays pretty much the same as it is now
// - micro will attempt to calculate during warmup how many operations can it perform in order to be done in X amount of time
// - this could use a floating-point time & memory structs for tracking
// - also, macro benchmark can track zero-elapsed-time samples
// - benchmark itself can have a 'title' property, that describes it
// - and it can have a RunFiltered method + base IBenchmark interface
//
// project idea: Diagnostics.Terminal
// - extends Benchmarking project with Terminal project capabilities (writes benchmark "events" to console)
