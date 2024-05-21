([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Identifiers)](https://www.nuget.org/packages/LfrlAnvil.Identifiers/)

# [LfrlAnvil.Identifiers](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Identifiers)

This project contains a generator of sequential 64-bit identifiers based on a timestamp.

### Examples

Following is an example of the simplest way to create a generator and generate a single identifier from it:
```csharp
// use an ITimestampProvider instance of your choice
// this provider will be used by the generator to get the current timestamp
// during new identifier creation
ITimestampProvider timestamps = ...;

// create a new generator instance
var generator = new IdentifierGenerator( timestamps );

// create the next identifier
var identifier = generator.Generate();

// it's also possible to extract a timestamp from an identifier
var timestamp = generator.GetTimestamp( identifier );
```

Identifiers consist of a 48-bit high value and a 16-bit low value.

The low value represents an ordinal from across all identifiers generated with the same high value.

The high value represents the timestamp, as a number of time units that have passed since the base timestamp of the generator.
By default, the time unit, or epsilon, is defined as **1 millisecond** and the base timestamp is the unix epoch.
However, those values can be changed through generator options.

Be advised though, that if generated identifiers are to be consistent across multiple applications
or multiple executions of the same application, then the time epsilon and the base timestamp must remain the same,
especially when at least one identifier has been generated already.
Otherwise, identifiers may get duplicated or their timestamp data may get corrupted.

Increasing the precision of a time epsilon has a side-effect of reducing the maximum timestamp that the generator can produce; the generator will run out of values sooner.
The default time epsilon (**1 milliscond**) allows to create identifiers in a span of ~8925 standard years (365 days per year),
with a maximum rate of 65 536 000 identifiers per second.
Changing this value to e.g. **1 microsecond** means that the generator will run out of values in just ~8.9 standard years,
but its maximum rate of generated identifiers will be 65 536 000 000 per second.
