using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Reactive.State.Tests" )]

// TODO:
// interfaces & classes for managing state value changes & validation
// changes should be emitted via event publishers
// should support primitive (atomic? might be a better name) values
// should support complex objects, consisting of multiple atoms
// should support collections of non-state objects
// should support collections of state objects (both atomic and complex)
// separate collection variables:
// CollectionVariable -> represents a dictionary (with optional ordered list?) of value objects
// ^ does not have to bother with listening to element changes
// ^ needs separate original elements & current elements collections to keep track of changes
// & CollectionVariableRoot -> represents a dictionary of nested variables
// ^ has to listen to element changes
// ^ does not need separate original elements & current elements collections, since change/validation tracking will be done via internal events
// ^ & differences per element are memorized by the element itself, since it is a variable
// ^ also, separate collections would require some weird copying of variables? not worth the trouble
