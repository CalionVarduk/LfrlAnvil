([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.MessageBroker.Server)](https://www.nuget.org/packages/LfrlAnvil.MessageBroker.Server/)

# [<img src="../../../../assets/logo.png" alt="logo" height="80"/>](../../../../assets/logo.png) [LfrlAnvil.MessageBroker.Core](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.MessageBroker/LfrlAnvil.MessageBroker.Server)

This project contains server-side message broker implementation.

### Documentation

Technical documentation can be found [here](https://calionvarduk.github.io/LfrlAnvil/api/LfrlAnvil.MessageBroker.Server/LfrlAnvil.MessageBroker.Server.html).

### Examples

Following is an example of creating a server:
```csharp
// create a server
// see 'MessageBrokerServerOptions' for available options
IPEndPoint localEndPoint = ...;
var server = new MessageBrokerServer(
    localEndPoint,
    MessageBrokerServerOptions.Default );

// starts the server, allowing clients to connect
await server.StartAsync();

// fetches a channel object, can also be queried by name
var channel = server.Channel.TryGetById( 1 );

// fetches a stream object, can also be queried by name
var stream = server.Streams.TryGetById( 1 );

// fetches a client object, can also be queried by name
var client = server.Clients.TryGetById( 1 );

// disconnects the client
// there also exists 'DeleteAsync' method
// which disconnects the client, if connected, and completely removes its resources
await client.DisconnectAsync();

// disconnects all clients and safely disposes the server
await server.DisposeAsync();
```
