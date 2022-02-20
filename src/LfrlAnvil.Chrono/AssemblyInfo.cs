using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Chrono.Tests" )]

// TODO: add Bounds<T> extensions, where T is ZonedDateTime (ZonedDay as well?), mostly for properly calculating Duration
// TODO: add BoundsRange<T> extensions, where T is ZonedDateTime (ZonedDay as well?), mostly for properly calculating Duration
// TODO: add ToBounds method to all interval-like structs (actually, returns a BoundsRange<T>, since start/end can be invalid/ambiguous)
// TODO: add ToUnsafeBounds method to all interval-like structs (returns Bounds<T>, from Start to End)
