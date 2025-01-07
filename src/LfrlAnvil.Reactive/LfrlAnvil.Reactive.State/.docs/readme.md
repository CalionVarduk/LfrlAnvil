([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Reactive.State)](https://www.nuget.org/packages/LfrlAnvil.Reactive.State/)

# [<img src="../../../../assets/logo.png" alt="logo" height="80"/>](../../../../assets/logo.png) [LfrlAnvil.Reactive.State](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Reactive/LfrlAnvil.Reactive.State)

This project contains a few functionalities related to state management.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Reactive.State/LfrlAnvil.Reactive.State.html).

### Examples

Following is an example of a `string` variable:
```csharp
// creates a new variable with initial 'foo' value,
// error validator that does not allow empty strings
// and warnings validator that warns about strings containing more than 10 characters
var variable = Variable.Create(
    initialValue: "foo",
    errorsValidator: FormattableValidators<string>.NotEmpty( "IsEmpty" ),
    warningsValidator: FormattableValidators<string>.MaxLength( 10, "LongText" ) );

// attaches a listener to the 'OnChange' event stream
variable.OnChange.Listen(
    EventListener.Create<VariableValueChangeEvent<string, ValidationMessage<string>>>(
        e => Console.WriteLine( $"'{e.PreviousValue}' => '{e.NewValue}', State: {e.NewState}" ) ) );

// attaches a listener to the 'OnValidate' event stream
variable.OnValidate.Listen(
    EventListener.Create<VariableValidationEvent<string, ValidationMessage<string>>>(
        e =>
        {
            var errors = string.Join( " & ", e.NewErrors.Select( m => $"({m})" ) );
            errors = errors.Length > 0 ? $"Errors: {errors}" : "No errors";

            var warnings = string.Join( " & ", e.NewWarnings.Select( m => $"({m})" ) );
            warnings = warnings.Length > 0 ? $"Warnings: {warnings}" : "No warnings";

            Console.WriteLine( $"{errors}, {warnings}" );
        } ) );

// changes the underlying value
variable.Change( "bar" );

// expected console output:
// 'foo' => 'bar', State: Changed, Dirty
// No errors, No warnings

// changes the underlying value again, to an invalid empty string
variable.Change( string.Empty );

// expected console output:
// 'bar' => '', State: Changed, Invalid, Dirty
// Errors: (Resource: 'IsEmpty', Parameters: 0), No warnings

// changes the underlying value again, to a string that causes a warning
variable.Change( "lorem ipsum" );

// expected console output:
// '' => 'lorem ipsum', State: Changed, Warning, Dirty
// No errors, Warnings: (Resource: 'LongText', Parameters: 1)

// changes the underlying value again, to the initial value
variable.Change( "foo" );

// expected console output:
// 'lorem ipsum' => 'foo', State: Dirty
// No errors, No warnings

// disposes the variable
variable.Dispose();
```

Following is an example of a collection variable:
```csharp
// creates a new collection variable with (string, int) pair elements,
// where each element is identified by their string key,
// with error validator that does not allow empty collections
// and element error validator that does not allow negative values
var variable = CollectionVariable.Create(
    initialElements: new[] { KeyValuePair.Create( "foo", 1 ) },
    keySelector: e => e.Key,
    errorsValidator: FormattableValidators<string>.NotEmpty<KeyValuePair<string, KeyValuePair<string, int>>>( "IsEmpty" )
        .For( (ICollectionVariableElements<string, KeyValuePair<string, int>, ValidationMessage<string>> e) => e ),
    elementErrorsValidator: FormattableValidators<string>.GreaterThanOrEqualTo( 0, "IsNegative" )
        .For( (KeyValuePair<string, int> e) => e.Value ) );

// attaches a listener to the 'OnChange' event stream
variable.OnChange.Listen(
    EventListener.Create<CollectionVariableChangeEvent<string, KeyValuePair<string, int>, ValidationMessage<string>>>(
        e =>
        {
            var added = string.Join(
                " & ",
                e.AddedElements.Select( s => $"(['{s.Element.Key}', {s.Element.Value}], State: {s.NewState})" ) );

            added = added.Length > 0 ? $"Added: {added}" : "No added elements";
            var removed = string.Join(
                " & ",
                e.RemovedElements.Select( s => $"(['{s.Element.Key}', {s.Element.Value}], State: {s.NewState})" ) );

            removed = removed.Length > 0 ? $"Removed: {removed}" : "No removed elements";
            var replaced = string.Join(
                " & ",
                e.ReplacedElements.Select(
                    s => $"(['{s.Element.Key}', {s.PreviousElement.Value} => {s.Element.Value}], State: {s.NewState})" ) );

            replaced = replaced.Length > 0 ? $"Replaced: {replaced}" : "No replaced elements";

            Console.WriteLine( $"{added}, {removed}, {replaced}" );
        } ) );

// attaches a listener to the 'OnValidate' event stream
variable.OnValidate.Listen(
    EventListener.Create<CollectionVariableValidationEvent<string, KeyValuePair<string, int>, ValidationMessage<string>>>(
        e =>
        {
            var errors = string.Join( " & ", e.NewErrors.Select( m => $"({m})" ) );
            errors = errors.Length > 0 ? $"Errors: {errors}" : "No errors";

            var warnings = string.Join( " & ", e.NewWarnings.Select( m => $"({m})" ) );
            warnings = warnings.Length > 0 ? $"Warnings: {warnings}" : "No warnings";

            Console.WriteLine( $"{errors}, {warnings}, State: {e.NewState}" );
        } ) );

// adds a new element
variable.Add( KeyValuePair.Create( "bar", 42 ) );

// expected console output:
// Added: (['bar', 42], State: Added), No removed elements, No replaced elements
// No errors, No warnings, State: Changed, Dirty

// adds one more invalid element and replaces existing 'foo' element
variable.AddOrReplace( new[] { KeyValuePair.Create( "qux", -1 ), KeyValuePair.Create( "foo", -2 ) } );

// expected console output:
// Added: (['qux', -1], State: Invalid, Added), No removed elements, Replaced: (['foo', 1 => -2], State: Changed, Invalid)
// No errors, No warnings, State: Changed, Invalid, Dirty

// removes two elements
variable.Remove( new[] { "qux", "foo" } );

// expected console output:
// No added elements, Removed: (['qux', -1], State: NotFound) & (['foo', -2], State: Removed), No replaced elements
// No errors, No warnings, State: Changed, Dirty

// clears the collection
variable.Clear();

// expected console output:
// No added elements, Removed: (['bar', 42], State: NotFound), No replaced elements
// Errors: (Resource: 'IsEmpty', Parameters: 0), No warnings, State: Changed, Invalid, Dirty

// sets elements to the initial collection
variable.Change( new[] { KeyValuePair.Create( "foo", 1 ) } );

// expected console output:
// Added: (['foo', 1], State: Default), No removed elements, No replaced elements
// No errors, No warnings, State: Dirty

// disposes the variable
variable.Dispose();
```

Following is an example of a variable root, that is a variable that contains other child variables:
```csharp
// creates a new variable root
var variable = new Root();

// attaches a listener to the 'OnChange' event stream
variable.OnChange.Listen(
    EventListener.Create<VariableRootChangeEvent<string>>( e => Console.WriteLine( $"ChangedKey: '{e.NodeKey}'" ) ) );

// attaches a listener to the 'OnValidate' event stream
variable.OnValidate.Listen(
    EventListener.Create<VariableRootValidationEvent<string>>(
        e => Console.WriteLine( $"ValidatedKey: '{e.NodeKey}', State: {e.NewState}" ) ) );

// attaches a listener to the 'OnChange' event stream of the 'Text' property
variable.Text.OnChange.Listen(
    EventListener.Create<VariableValueChangeEvent<string, ValidationMessage<string>>>(
        e => Console.WriteLine( $"[Text] '{e.PreviousValue}' => '{e.NewValue}', State: {e.NewState}" ) ) );

// attaches a listener to the 'OnValidate' event stream of the 'Text' property
variable.Text.OnValidate.Listen(
    EventListener.Create<VariableValidationEvent<string, ValidationMessage<string>>>(
        e =>
        {
            var errors = string.Join( " & ", e.NewErrors.Select( m => $"({m})" ) );
            errors = errors.Length > 0 ? $"Errors: {errors}" : "No errors";

            var warnings = string.Join( " & ", e.NewWarnings.Select( m => $"({m})" ) );
            warnings = warnings.Length > 0 ? $"Warnings: {warnings}" : "No warnings";

            Console.WriteLine( $"[Text] {errors}, {warnings}" );
        } ) );

// attaches a listener to the 'OnChange' event stream of the 'Ordinal' property
variable.Ordinal.OnChange.Listen(
    EventListener.Create<VariableValueChangeEvent<int, ValidationMessage<string>>>(
        e => Console.WriteLine( $"[Ordinal] {e.PreviousValue} => {e.NewValue}, State: {e.NewState}" ) ) );

// attaches a listener to the 'OnValidate' event stream of the 'Ordinal' property
variable.Ordinal.OnValidate.Listen(
    EventListener.Create<VariableValidationEvent<int, ValidationMessage<string>>>(
        e =>
        {
            var errors = string.Join( " & ", e.NewErrors.Select( m => $"({m})" ) );
            errors = errors.Length > 0 ? $"Errors: {errors}" : "No errors";

            var warnings = string.Join( " & ", e.NewWarnings.Select( m => $"({m})" ) );
            warnings = warnings.Length > 0 ? $"Warnings: {warnings}" : "No warnings";

            Console.WriteLine( $"[Ordinal] {errors}, {warnings}" );
        } ) );

// sets 'Text' value
variable.SetText( "foo" );

// expected console output:
// ChangedKey: 'Text'
// [Text] '' => 'foo', State: Changed, Dirty
// ValidatedKey: 'Text', State: Changed, Invalid, Dirty
// [Text] No errors, No warnings

// sets 'Ordinal' value
variable.SetOrdinal( 42 );

// expected console output:
// ChangedKey: 'Ordinal'
// [Ordinal] 0 => 42, State: Changed, Dirty
// ValidatedKey: 'Ordinal', State: Changed, Dirty
// [Ordinal] No errors, No warnings

// disposes the variable
variable.Dispose();
```

There also exists a collection version of a variable root.
