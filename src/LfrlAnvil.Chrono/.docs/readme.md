([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Chrono)](https://www.nuget.org/packages/LfrlAnvil.Chrono/)

# [<img src="../../../assets/logo.png" alt="logo" height="80"/>](../../../assets/logo.png) [LfrlAnvil.Chrono](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Chrono)

This project contains functionalities related to date & time, as well as timezones.
Most of them work directly on dotnet's [DateTime](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-7.0)
and [TimeZoneInfo](https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo?view=net-7.0) types.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.Chrono/LfrlAnvil.Chrono.html).

### Examples

Following are a few examples of date & time structures:

```csharp
// creates a new timestamp provider
var timestamps = new TimestampProvider();

// returns the current timestamp, which represents the number of ticks elapsed since unix epoch
var timestamp = timestamps.GetNow();

// target timezone
var timezone = TimeZoneInfo.FindSystemTimeZoneById( ... );

// creates a new zoned date time provider, with the desired timezone
var clock = new ZonedClock( timezone );

// returns the current date & time, in the clock's timezone
var datetime = clock.GetNow();

// adds a duration to datetime
// duration represents elapsed time in ticks
var a = datetime + Duration.FromHours( 2 );

// adds a period to datetime
// period represents elapsed time in calendar-related chronological units, such as years, months etc.
var b = datetime + Period.FromMonths( 1 ).AddWeeks( 2 ).AddMinutes( 420 );

// sets a time of day of datetime to 12:00:00
var c = datetime.SetTimeOfDay( TimeOfDay.Mid );

// extracts a zoned day instance from datetime
// there are also other zoned objects, such as year, month and week
var day = datetime.GetDay();
```

This project also includes implementations of caches, whose entries have a limited lifetime:

```csharp
// creates a new timestamp provider
var timestamps = new TimestampProvider();

// timestamp of the creation of cache
var start = timestamps.GetNow();

// creates a new empty cache with 'string' key and 'int' value, with entry lifetime equal to '2 minutes'
var cache = new LifetimeCache<string, int>( startTimestamp: start, lifetime: Duration.FromMinutes( 2 ) );

// adds an entry to the cache, whose lifetime ends at 'start + 2 minutes'
cache.AddOrUpdate( "foo", 42 );

// moves the cache's current timestamp forward by '30 seconds', to 'start + 30 seconds' timestamp
// this means that the 'foo' entry has '1 minute and 30 seconds' left
cache.Move( Duration.FromSeconds( 30 ) );

// gets the value associated with 'foo' key, which should return 42
// this also resets entry's lifetime back to '2 minutes',
// which means that it will now be removed at 'start + 2 minutes and 30 seconds' timestamp
var foo = cache["foo"];

// moves the cache's current timestamp forward by '2 minutes', to 'start + 2 minutes and 30 seconds' timestamp
// this will also remove the 'foo' entry from cache
cache.Move( Duration.FromMinutes( 2 ) );

// checks whether or not the 'foo' entry exits, should return false
var exists = cache.ContainsKey( "foo" );
```

There also exists a version of lifetime cache that allows to set each entry's lifetime individually.
