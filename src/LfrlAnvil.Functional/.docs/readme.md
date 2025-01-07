([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Functional)](https://www.nuget.org/packages/LfrlAnvil.Functional/)

# [<img src="../../../assets/logo.png" alt="logo" height="80"/>](../../../assets/logo.png) [LfrlAnvil.Functional](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Functional)

This project contains a few functional programming functionalities.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Functional/LfrlAnvil.Functional.html).

### Examples

Following is an example of a `Maybe` (or `Option`) monad:
```csharp
// creates a maybe instance with a value
var some = Maybe.Some( "foo" );

// creates a maybe instance without a value
var none = Maybe<string>.None;

// this will throw an exception, because the value is null
var _ = Maybe.Some( ( string? )null );

// this is the safe way to create a maybe instance from a nullable value
var safe = (( string? )null).ToMaybe();

// converts maybe of string to maybe of int, using provided 'some' and 'none' delegates
// the 'some' delegate will be invoked only when the source maybe has a value,
// otherwise the 'none' delegate will be invoked
var bindResult = safe.Bind( some: value => int.TryParse( value, out var r ) ? r : Maybe<int>.None, none: () => Maybe<int>.None );

// converts maybe of string to string, using provided 'some' and 'none' delegates
// the 'some' delegate will be invoked only when the source maybe has a value,
// otherwise the 'none' delegate will be invoked
var matchResult = safe.Match( some: value => value, none: () => string.Empty );

// gets an underlying value or returns the provided argument, if maybe does not have a value
// this is equivalent to the above 'Match' invocation
var result = safe.GetValueOrDefault( string.Empty );
```

There are other monads as well, like `Either`, `Erratic` (represents either a returned value or a thrown exception),
`TypeCast` or `Mutation` (represents a change of value). There is also the `Nil` type, which represents a lack of value.
