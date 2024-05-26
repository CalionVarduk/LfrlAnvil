([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Reactive.Core)](https://www.nuget.org/packages/LfrlAnvil.Reactive.Core/)

# [LfrlAnvil.Reactive.Core](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Reactive/LfrlAnvil.Reactive.Core)

This project contains a few reactive programming functionalities.

### Examples

Following is an example of an event publisher usage:
```csharp
// creates a new event publisher with events of 'string' type
var publisher = new EventPublisher<string>();

// attaches a new event listener to the publisher
var subscriber = publisher.Listen( EventListener.Create<string>( e => Console.WriteLine( $"First: '{e}'" ) ) );

// attaches another event listener to decorated version of the publisher
// this listener will be invoked only for events that contain
// at least two characters and that can be parsed to 32-bit integers
publisher
    .Where( e => e.Length > 1 )
    .Select( e => int.TryParse( e, out var r ) ? r : ( int? )null )
    .WhereNotNull()
    .Listen( EventListener.Create<int>( e => Console.WriteLine( $"Second: {e}" ) ) );

// publishes a few events
publisher.Publish( "foo" );
publisher.Publish( "1" );
publisher.Publish( "123" );
publisher.Publish( "1r" );
publisher.Publish( "bar" );
publisher.Publish( "42" );

// expected console output:
// First: 'foo'
// First: '1'
// First: '123'
// Second: 123
// First: '1r'
// First: 'bar'
// First: '42'
// Second: 42

// removes the first event listener from the publisher
subscriber.Dispose();

// publishes a few more events
publisher.Publish( "qux" );
publisher.Publish( "10" );

// expected console output:
// Second: 10

// removes all listeners from the publisher and disposes it
publisher.Dispose();
```

There exist plenty of other built-in event source `decorators`.
It's also possible to create entirely new `decorators`.

Following is an example of conversions between event streams and dotnet tasks:
```csharp
// creates a new event publisher with events of 'string' type
var publisher = new EventPublisher<string>();

// creates a new task from the event stream
// this task will complete with the last emitted event,
// once the underlying event stream is disposed
var task = publisher.ToTask( CancellationToken.None );

// publishes a few events
publisher.Publish( "foo" );
publisher.Publish( "bar" );
publisher.Publish( "qux" );

// at this point, the task is still running
// disposing the publisher will cause the task to complete
publisher.Dispose();

// gets the task's result, which should be equal to 'qux'
var result = await task;

// ----------

// creates a new task
var taskSource = new TaskCompletionSource<string>();
var task = taskSource.Task;

// creates a new event source based on the task
// provided task factory will be invoked for each listener
// and listeners will react to task's result, once it completes
var source = EventSource.FromTask( _ => task );

// attaches a new listener to the task event source
source.Listen( EventListener.Create<FromTask<string>>( e => Console.WriteLine( $"Task: '{e.Result}'" ) ) );

// completes the task with a result
taskSource.SetResult( "foo" );

// expected console output:
// Task: 'foo'
```

Following is an example of an in-memory event exchange:
```csharp
// creates a new empty exchange, where publishers are identified by their event type
var exchange = new EventExchange();

// registers a new publisher with events of 'string' type
exchange.RegisterPublisher<string>();

// registers a new publisher with events of 'string' type
exchange.RegisterPublisher<int>();

// attaches listeners to registered publishers
exchange.Listen( EventListener.Create<string>( e => Console.WriteLine( $"String: '{e}'" ) ) );
exchange.Listen( EventListener.Create<int>( e => Console.WriteLine( $"Int: '{e}'" ) ) );

// publishes a few events
exchange.Publish( "foo" );
exchange.Publish( 42 );
exchange.Publish( "bar" );
exchange.Publish( "qux" );
exchange.Publish( 123 );

// expected console output:
// String: 'foo'
// Int: 42
// String: 'bar'
// String: 'qux'
// Int: 123
```
