([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.MessageBroker.Client)](https://www.nuget.org/packages/LfrlAnvil.MessageBroker.Client/)

# [<img src="../../../../assets/logo.png" alt="logo" height="80"/>](../../../../assets/logo.png) [LfrlAnvil.MessageBroker.Core](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.MessageBroker/LfrlAnvil.MessageBroker.Client)

This project contains client-side message broker implementation, which follows at-least-once delivery semantics.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.MessageBroker.Client/LfrlAnvil.MessageBroker.Client.html).

### Examples

Following is an example of creating a client and interacting with the server:
```csharp
// create a client with 'my-client' name
// see 'MessageBrokerClientOptions' for available options
IPEndPoint serverEndpoint = ...;
var client = new MessageBrokerClient(
    serverEndpoint,
    "my-client",
    MessageBrokerClientOptions.Default );

// starts the client and connects to the server
await client.StartAsync();

// binds a message publisher to the 'foo' channel
var publisher = (await client.Publishers.BindAsync( "foo" ))
    .GetValueOrThrow()!.Value
    .Publisher;

// binds a message listener to the 'foo' channel
// see 'MessageBrokerListenerOptions' for available options
var listener = (await client.Listeners.BindAsync(
        "foo",
        (args, ct) =>
        {
            // callback invoked when a message intended for this listener arrives from the server
            // args also allows to easily send either an ACK or a NACK, if enabled
            // or 'SendMessageAckAsync' and 'SendNegativeMessageAckAsync' listener methods can be used instead
            Console.WriteLine( args.ToString() );
            return ValueTask.CompletedTask;
        },
        MessageBrokerListenerOptions.Default ))
    .GetValueOrThrow()!.Value
    .Listener;

// pushes a message to the 'foo' channel
// for more efficient memory-wise message pushing, see 'GetPushContext' method
await publisher.PushAsync( new byte[] { 1, 2, 3 } );

// unbinds the publisher from the channel
await publisher.UnbindAsync();

// unbinds the listener from the channel
await listener.UnbindAsync();

// closes the connection to the server and disposes the client
await client.DisposeAsync();
```
