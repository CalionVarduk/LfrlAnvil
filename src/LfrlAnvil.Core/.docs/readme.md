([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Core)](https://www.nuget.org/packages/LfrlAnvil.Core/)

# [<img src="../../../assets/logo.png" alt="logo" height="80"/>](../../../assets/logo.png) [LfrlAnvil.Core](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Core)

This project contains a bunch of lightweight core functionalities, used by other `LfrlAnvil` projects.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Core/LfrlAnvil.html).

### Examples

Following are examples of some of the more interesting functionalities:
```csharp
var value = 42;

// the 'Ensure' static class contains a set of assertion methods
Ensure.IsGreaterThan( value, 40 );
Ensure.IsInRange( value, 4, 104 );
Ensure.IsNotNull( ( int? )value );
Ensure.IsNotEmpty( value.ToString() );

// ----------
// the 'Assume' static class contains a set of assertion methods for 'DEBUG' mode only
Assume.IsGreaterThan( value, 40 );
Assume.IsInRange( value, 4, 104 );
Assume.IsNotNull( value );
Assume.IsNotEmpty( value.ToString() );

// ----------
// the generic 'Bitmask' struct allows to manipulate bitmasks in a more managed way
[Flags]
public enum Foo
{
    None = 0,
    A = 1,
    B = 2,
    C = 4
}

// creates an object equivalent to 'Foo.None'
var bitmask = Bitmask<Foo>.Empty;

// performs bitwise-or operations
// should returns an object equivalent to 'Foo.A | Foo.B'
bitmask = bitmask.Set( Foo.A ).Set( Foo.B );

// performs bitwise-and operations
// should return an object equivalent to 'Foo.A'
bitmask = bitmask.Intersect( Foo.A | Foo.C );

// ----------
// the generic 'Bounds' struct represents a range of values
// there also exists the 'BoundsRange' struct, that represents an ordered collection
// of disjointed bounds instances
var bounds = new Bounds<int>( min: 4, max: 104 );

// checks whether or not 42 is contained in [4, 104] range, should return true
var contains = bounds.Contains( 42 );

// ----------
// the Interlocked* structures represent atomic values
var b = new InterlockedBoolean( false );

// atomically writes true to 'b' and returns information about whether or not the value has changed,
// which in this case should return true
var changed = b.WriteTrue();

// ----------
// the 'Fixed' structure represents a number that uses fixed-point precision arithmetic
// this creates a new 'Fixed' instance that represents 42.1230 number
// these numbers are internally stored as 64-bit signed integers
var f = Fixed.Create( 42.123m, precision: 4 );

// adds two 'Fixed' numbers together, which in this case should result in 42.2464 number
f = f + Fixed.Create( 0.1234m, precision: 4 );

// ----------
// creates a new cache with 'string' key and 'int' value, with maximum capacity for 3 entries
// for the most part, caches behave like dictionaries, with the additional entry count limit
var cache = new Cache<string, int>( capacity: 3 );

// adds 3 entries to the cache, which fills it up to its maximum capacity
cache.AddOrUpdate( "foo", 42 );
cache.AddOrUpdate( "bar", 123 );
cache.AddOrUpdate( "qux", -1 );

// adding another entry will cause the oldest entry (in this case, the 'foo' entry) to be removed
cache.AddOrUpdate( "lorem", 246 );

// currently, cache consists of ('bar', 123) => ('qux', -1) => ('lorem', 246) sequence of entries
// fetching a value from the cache also moves it to the end of the sequence,
// effectively making it the newest entry
// the following should return 123 and rearrange the sequence to:
// ('qux', -1) => ('lorem', 246) => ('bar', 123)
var result = cache["bar"];

// entries can also be removed manually, like so:
cache.Remove( "lorem" );
```

There are also a plethora of other minor functionalities and extension methods.
