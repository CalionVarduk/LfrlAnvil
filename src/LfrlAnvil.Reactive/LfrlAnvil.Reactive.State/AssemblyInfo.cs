using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Reactive.State.Tests" )]

// TODO:
// interfaces & classes for managing state value changes & validation
// changes should be emitted via event publishers
// should support primitive (atomic? might be a better name) values
// should support complex objects, consisting of multiple atoms
// should support collections of non-state objects
// should support collections of state objects (both atomic and complex)
