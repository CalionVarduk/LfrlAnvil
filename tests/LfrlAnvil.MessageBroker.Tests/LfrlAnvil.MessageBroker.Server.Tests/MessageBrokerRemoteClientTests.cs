using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerRemoteClientTests : TestsBase
{
    [Fact]
    public async Task Start_ShouldRegisterClientAndEstablishHandshake()
    {
        var waitingForMessageCount = 0;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ =>
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerRemoteClientEventType.WaitingForMessage )
                            {
                                if ( ++waitingForMessageCount == 3 )
                                    endSource.Complete();
                            }
                        } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                c.SendConfirmHandshakeResponse();
            },
            client );

        await clientTask;
        var remoteClient = server.Clients.TryGetById( 1 );
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 1 );
            server.Clients.GetAll().Should().BeSequentiallyEqualTo( remoteClient! );
            remoteClient.Should().NotBeNull();
            remoteClient.Should().BeSameAs( server.Clients.TryGetByName( "test" ) );
            (remoteClient?.Server).Should().BeSameAs( server );
            (remoteClient?.Id).Should().Be( 1 );
            (remoteClient?.Name).Should().Be( "test" );
            (remoteClient?.LocalEndPoint).Should().NotBeNull();
            (remoteClient?.RemoteEndPoint).Should().NotBeNull();
            (remoteClient?.IsLittleEndian).Should().BeTrue();
            (remoteClient?.MessageTimeout).Should().Be( Duration.FromSeconds( 1 ) );
            (remoteClient?.PingInterval).Should().Be( Duration.FromSeconds( 10 ) );
            (remoteClient?.State).Should().Be( MessageBrokerRemoteClientState.Running );

            AssertClientData(
                client.GetAllReceived(),
                (Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload,
                    MessageBrokerClientEndpoint.HandshakeAcceptedResponse) );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[1::<ROOT>] [Created] From {remoteClient?.RemoteEndPoint}",
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeRequest",
                    "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 1 second(s), PingInterval = 10 second(s))",
                    "[1::'test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [WaitingForMessage]",
                    "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[1::'test'::<ROOT>] [WaitingForMessage]" );
        }
    }

    [Fact]
    public async Task Start_ShouldRegisterClientAndEstablishHandshake_WithStreamDecoratorAndThrowingEventHandler()
    {
        NetworkStream? stream = null;
        var waitingForMessageCount = 0;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ =>
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerRemoteClientEventType.WaitingForMessage )
                            {
                                if ( ++waitingForMessageCount == 3 )
                                    endSource.Complete();
                            }

                            throw new Exception( "ignored" );
                        } )
                .SetStreamDecorator(
                    (_, ns) =>
                    {
                        stream = ns;
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "foo", Duration.FromSeconds( 1.5 ), Duration.FromSeconds( 15 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                c.SendConfirmHandshakeResponse();
            },
            client );

        await clientTask;
        var remoteClient = server.Clients.TryGetById( 1 );
        await endSource.Task;

        using ( new AssertionScope() )
        {
            stream.Should().NotBeNull();
            server.Clients.Count.Should().Be( 1 );
            server.Clients.GetAll().Should().BeSequentiallyEqualTo( remoteClient! );
            remoteClient.Should().NotBeNull();
            remoteClient.Should().BeSameAs( server.Clients.TryGetByName( "foo" ) );
            (remoteClient?.Server).Should().BeSameAs( server );
            (remoteClient?.Id).Should().Be( 1 );
            (remoteClient?.Name).Should().Be( "foo" );
            (remoteClient?.LocalEndPoint).Should().NotBeNull();
            (remoteClient?.RemoteEndPoint).Should().NotBeNull();
            (remoteClient?.IsLittleEndian).Should().BeTrue();
            (remoteClient?.MessageTimeout).Should().Be( Duration.FromSeconds( 1.5 ) );
            (remoteClient?.PingInterval).Should().Be( Duration.FromSeconds( 15 ) );
            (remoteClient?.State).Should().Be( MessageBrokerRemoteClientState.Running );

            AssertClientData(
                client.GetAllReceived(),
                (Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload,
                    MessageBrokerClientEndpoint.HandshakeAcceptedResponse) );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[1::<ROOT>] [Created] From {remoteClient?.RemoteEndPoint}",
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 17] Begin handling HandshakeRequest",
                    "[1::'foo'::<ROOT>] [MessageAccepted] [PacketLength: 17] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 1.5 second(s), PingInterval = 15 second(s))",
                    "[1::'foo'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'foo'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'foo'::<ROOT>] [WaitingForMessage]",
                    "[1::'foo'::<ROOT>] [MessageReceived] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[1::'foo'::<ROOT>] [MessageAccepted] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[1::'foo'::<ROOT>] [WaitingForMessage]" );
        }
    }

    [Fact]
    public async Task Start_ShouldRegisterManyClientsAndEstablishHandshake()
    {
        var waitingForMessageCount = new InterlockedInt32( 0 );
        var endSource = new SafeTaskCompletionSource();
        var serverLogs = new EventLogger();
        var clientLogIndex = new InterlockedInt32( 0 );
        var clientLogs = new[] { new EventLogger(), new EventLogger() };
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetAcceptableMessageTimeout( Bounds.Create( Duration.FromSeconds( 1 ), Duration.FromSeconds( 1.5 ) ) )
                .SetAcceptablePingInterval( Bounds.Create( Duration.FromSeconds( 10 ), Duration.FromSeconds( 15 ) ) )
                .SetEventHandler( serverLogs.Add )
                .SetClientEventHandlerFactory(
                    _ =>
                    {
                        var logs = clientLogs[clientLogIndex.Increment() - 1];
                        return e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerRemoteClientEventType.WaitingForMessage )
                            {
                                if ( waitingForMessageCount.Increment() == 6 )
                                    endSource.Complete();
                            }
                        };
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client1 = new ClientMock();
        var clientTask1 = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "foo", Duration.FromSeconds( 0.5 ), Duration.FromSeconds( 5 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                c.SendConfirmHandshakeResponse();
            },
            client1 );

        await clientTask1;

        using var client2 = new ClientMock();
        var clientTask2 = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "bar", Duration.FromSeconds( 2 ), Duration.FromSeconds( 20 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                c.SendConfirmHandshakeResponse();
            },
            client2 );

        await clientTask2;
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 2 );
            server.Clients.GetAll().Should().BeSequentiallyEqualTo( remoteClient1!, remoteClient2! );

            remoteClient1.Should().NotBeNull();
            remoteClient1.Should().BeSameAs( server.Clients.TryGetByName( "foo" ) );
            (remoteClient1?.Server).Should().BeSameAs( server );
            (remoteClient1?.Id).Should().Be( 1 );
            (remoteClient1?.Name).Should().Be( "foo" );
            (remoteClient1?.LocalEndPoint).Should().NotBeNull();
            (remoteClient1?.RemoteEndPoint).Should().NotBeNull();
            (remoteClient1?.IsLittleEndian).Should().BeTrue();
            (remoteClient1?.MessageTimeout).Should().Be( Duration.FromSeconds( 1 ) );
            (remoteClient1?.PingInterval).Should().Be( Duration.FromSeconds( 10 ) );
            (remoteClient1?.State).Should().Be( MessageBrokerRemoteClientState.Running );

            remoteClient2.Should().NotBeNull();
            remoteClient2.Should().BeSameAs( server.Clients.TryGetByName( "bar" ) );
            (remoteClient2?.Server).Should().BeSameAs( server );
            (remoteClient2?.Id).Should().Be( 2 );
            (remoteClient2?.Name).Should().Be( "bar" );
            (remoteClient2?.LocalEndPoint).Should().NotBeNull();
            (remoteClient2?.RemoteEndPoint).Should().NotBeNull();
            (remoteClient2?.IsLittleEndian).Should().BeTrue();
            (remoteClient2?.MessageTimeout).Should().Be( Duration.FromSeconds( 1.5 ) );
            (remoteClient2?.PingInterval).Should().Be( Duration.FromSeconds( 15 ) );
            (remoteClient2?.State).Should().Be( MessageBrokerRemoteClientState.Running );

            AssertClientData(
                client1.GetAllReceived(),
                (Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload,
                    MessageBrokerClientEndpoint.HandshakeAcceptedResponse) );

            AssertClientData(
                client2.GetAllReceived(),
                (Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload,
                    MessageBrokerClientEndpoint.HandshakeAcceptedResponse) );

            serverLogs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(1 second(s) : 1.5 second(s)), AcceptablePingInterval = Bounds(10 second(s) : 15 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            clientLogs[0]
                .GetAllClient()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[1::<ROOT>] [Created] From {remoteClient1?.RemoteEndPoint}",
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 17] Begin handling HandshakeRequest",
                    "[1::'foo'::<ROOT>] [MessageAccepted] [PacketLength: 17] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 1 second(s), PingInterval = 10 second(s))",
                    "[1::'foo'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'foo'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'foo'::<ROOT>] [WaitingForMessage]",
                    "[1::'foo'::<ROOT>] [MessageReceived] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[1::'foo'::<ROOT>] [MessageAccepted] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[1::'foo'::<ROOT>] [WaitingForMessage]" );

            clientLogs[1]
                .GetAllClient()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[2::<ROOT>] [Created] From {remoteClient2?.RemoteEndPoint}",
                    "[2::<ROOT>] [WaitingForMessage]",
                    "[2::<ROOT>] [MessageReceived] [PacketLength: 17] Begin handling HandshakeRequest",
                    "[2::'bar'::<ROOT>] [MessageAccepted] [PacketLength: 17] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 1.5 second(s), PingInterval = 15 second(s))",
                    "[2::'bar'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[2::'bar'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[2::'bar'::<ROOT>] [WaitingForMessage]",
                    "[2::'bar'::<ROOT>] [MessageReceived] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[2::'bar'::<ROOT>] [MessageAccepted] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[2::'bar'::<ROOT>] [WaitingForMessage]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenStreamDecoratorThrows()
    {
        var exception = new Exception( "invalid" );
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } )
                .SetStreamDecorator( (_, _) => ValueTask.FromException<Stream>( exception ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .ElementAtOrDefault( 1 )
                .Should()
                .StartWith(
                    """
                    [1::<ROOT>] [Unexpected] Encountered an error:
                    System.Exception: invalid
                    """ );

            logs.GetAllClient()
                .TakeLast( 2 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [Disposing]",
                    "[1::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenStreamDecoratorDisposesClient()
    {
        var endSource = new SafeTaskCompletionSource<(MessageBrokerRemoteClient Client, Task DisposeTask)>();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetStreamDecorator(
                    (c, ns) =>
                    {
                        endSource.Complete( (c, Task.Factory.StartNew( c.Dispose )) );
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
            },
            client );

        await clientTask;
        var (remoteClient, disposeTask) = await endSource.Task;
        await disposeTask;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );
            remoteClient.LocalEndPoint.Should().BeNull();
            remoteClient.RemoteEndPoint.Should().BeNull();
            remoteClient.State.Should().Be( MessageBrokerRemoteClientState.Disposed );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenWritingHandshakeResponseToStreamFails()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
        Stream? stream = null;

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.SendingMessage )
                            stream?.Dispose();
                        else if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } )
                .SetStreamDecorator(
                    (_, ns) =>
                    {
                        stream = ns;
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Skip( 1 )
                .Take( 4 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeRequest",
                    "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 1 second(s), PingInterval = 10 second(s))",
                    "[1::'test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse" );

            logs.GetAllClient()
                .ElementAtOrDefault( 5 )
                .Should()
                .StartWith(
                    """
                    [1::'test'::<ROOT>] [SendingMessage] [PacketLength: 18] Encountered an error:
                    System.ObjectDisposedException:
                    """ );

            logs.GetAllClient()
                .Skip( 6 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::'test'::<ROOT>] [Disposing]",
                    "[1::'test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientDisconnectsAfterEstablishingConnection()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.Dispose();
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient().ElementAtOrDefault( 1 ).Should().Be( "[1::<ROOT>] [WaitingForMessage]" );

            logs.GetAllClient()
                .ElementAtOrDefault( 2 )
                .Should()
                .StartWith(
                    """
                    [1::<ROOT>] [WaitingForMessage] Encountered an error:
                    System.IO.EndOfStreamException:
                    """ );

            logs.GetAllClient()
                .Skip( 3 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [Disposing]",
                    "[1::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientDisconnectsAfterReceivingHandshakeResponse()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                c.Dispose();
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Skip( 1 )
                .Take( 6 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeRequest",
                    "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 1 second(s), PingInterval = 10 second(s))",
                    "[1::'test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [WaitingForMessage]" );

            logs.GetAllClient()
                .ElementAtOrDefault( 7 )
                .Should()
                .StartWith(
                    """
                    [1::'test'::<ROOT>] [WaitingForMessage] Encountered an error:
                    System.IO.EndOfStreamException:
                    """ );

            logs.GetAllClient()
                .Skip( 8 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::'test'::<ROOT>] [Disposing]",
                    "[1::'test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestEndpoint()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendConfirmHandshakeResponse();
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Skip( 1 )
                .Take( 2 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 5] ConfirmHandshakeResponse" );

            logs.GetAllClient()
                .ElementAtOrDefault( 3 )
                .Should()
                .StartWith(
                    """
                    [1::<ROOT>] [MessageRejected] [PacketLength: 5] Encountered an error:
                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid ConfirmHandshakeResponse with payload 4277993985 from client [1] ''. Encountered 1 error(s):
                    1. Received unexpected server endpoint.
                    """ );

            logs.GetAllClient()
                .Skip( 4 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [Disposing]",
                    "[1::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), payload: 8 );
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Skip( 1 )
                .Take( 2 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 13] Begin handling HandshakeRequest" );

            logs.GetAllClient()
                .ElementAtOrDefault( 3 )
                .Should()
                .StartWith(
                    """
                    [1::<ROOT>] [MessageRejected] [PacketLength: 13] Encountered an error:
                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid HandshakeRequest with payload 8 from client [1] ''. Encountered 1 error(s):
                    1. Packet length is invalid.
                    """ );

            logs.GetAllClient()
                .Skip( 4 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [Disposing]",
                    "[1::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestWithTooLongName()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( new string( 'x', 513 ), Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload );
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            AssertClientData(
                client.GetAllReceived(),
                (Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload,
                    MessageBrokerClientEndpoint.HandshakeRejectedResponse) );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Skip( 1 )
                .Take( 2 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 527] Begin handling HandshakeRequest" );

            logs.GetAllClient()
                .ElementAtOrDefault( 3 )
                .Should()
                .StartWith(
                    """
                    [1::<ROOT>] [MessageRejected] [PacketLength: 527] Encountered an error:
                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid HandshakeRequest with payload 522 from client [1] ''. Encountered 1 error(s):
                    1. Expected name length to be in [1, 512] range but found 513.
                    """ );

            logs.GetAllClient()
                .Skip( 4 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [SendingMessage] [PacketLength: 6] HandshakeRejectedResponse",
                    "[1::<ROOT>] [MessageSent] [PacketLength: 6] HandshakeRejectedResponse",
                    "[1::<ROOT>] [Disposing]",
                    "[1::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestWithEmptyName()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( string.Empty, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload );
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            AssertClientData(
                client.GetAllReceived(),
                (Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload,
                    MessageBrokerClientEndpoint.HandshakeRejectedResponse) );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Skip( 1 )
                .Take( 2 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 14] Begin handling HandshakeRequest" );

            logs.GetAllClient()
                .ElementAtOrDefault( 3 )
                .Should()
                .StartWith(
                    """
                    [1::<ROOT>] [MessageRejected] [PacketLength: 14] Encountered an error:
                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid HandshakeRequest with payload 9 from client [1] ''. Encountered 1 error(s):
                    1. Expected name length to be in [1, 512] range but found 0.
                    """ );

            logs.GetAllClient()
                .Skip( 4 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [SendingMessage] [PacketLength: 6] HandshakeRejectedResponse",
                    "[1::<ROOT>] [MessageSent] [PacketLength: 6] HandshakeRejectedResponse",
                    "[1::<ROOT>] [Disposing]",
                    "[1::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestWithDuplicatedName()
    {
        var endSource = new SafeTaskCompletionSource();
        var serverLogs = new EventLogger();
        var clientLogIndex = new InterlockedInt32( 0 );
        var clientLogs = new[] { new EventLogger(), new EventLogger() };
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( serverLogs.Add )
                .SetClientEventHandlerFactory(
                    _ =>
                    {
                        var index = clientLogIndex.Increment() - 1;
                        var logs = clientLogs[index];
                        return e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerRemoteClientEventType.Disposed && index == 1 )
                                endSource.Complete();
                        };
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client1 = new ClientMock();
        var clientTask1 = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                c.SendConfirmHandshakeResponse();
            },
            client1 );

        await clientTask1;

        using var client2 = new ClientMock();
        var clientTask2 = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload );
            },
            client2 );

        await clientTask2;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 1 );

            AssertClientData(
                client1.GetAllReceived(),
                (Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload,
                    MessageBrokerClientEndpoint.HandshakeAcceptedResponse) );

            AssertClientData(
                client2.GetAllReceived(),
                (Protocol.PacketHeader.Length + Protocol.HandshakeRejectedResponse.Payload,
                    MessageBrokerClientEndpoint.HandshakeRejectedResponse) );

            serverLogs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            clientLogs[1]
                .GetAllClient()
                .Skip( 1 )
                .Take( 2 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[2::<ROOT>] [WaitingForMessage]",
                    "[2::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeRequest" );

            clientLogs[1]
                .GetAllClient()
                .ElementAtOrDefault( 3 )
                .Should()
                .StartWith(
                    """
                    [2::'test'::<ROOT>] [MessageRejected] [PacketLength: 18] Encountered an error:
                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerDuplicateClientNameException: Client with name 'test' already exists.
                    """ );

            clientLogs[1]
                .GetAllClient()
                .Skip( 4 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[2::'test'::<ROOT>] [SendingMessage] [PacketLength: 6] HandshakeRejectedResponse",
                    "[2::'test'::<ROOT>] [MessageSent] [PacketLength: 6] HandshakeRejectedResponse",
                    "[2::'test'::<ROOT>] [Disposing]",
                    "[2::'test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidConfirmHandshakeResponseEndpoint()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Skip( 1 )
                .Take( 7 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeRequest",
                    "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 1 second(s), PingInterval = 10 second(s))",
                    "[1::'test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [WaitingForMessage]",
                    "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 218103813] Begin handling HandshakeRequest" );

            logs.GetAllClient()
                .ElementAtOrDefault( 8 )
                .Should()
                .StartWith(
                    """
                    [1::'test'::<ROOT>] [MessageRejected] [PacketLength: 218103813] Encountered an error:
                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid HandshakeRequest with payload 218103808 from client [1] 'test'. Encountered 1 error(s):
                    1. Received unexpected server endpoint.
                    """ );

            logs.GetAllClient()
                .Skip( 9 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::'test'::<ROOT>] [Disposing]",
                    "[1::'test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidConfirmHandshakeResponsePayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                c.SendConfirmHandshakeResponse( payload: 1 );
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[WaitingForClient]",
                    "[WaitingForClient]" );

            logs.GetAllClient()
                .Skip( 1 )
                .Take( 7 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeRequest",
                    "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 1 second(s), PingInterval = 10 second(s))",
                    "[1::'test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [WaitingForMessage]",
                    "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] ConfirmHandshakeResponse" );

            logs.GetAllClient()
                .ElementAtOrDefault( 8 )
                .Should()
                .StartWith(
                    """
                    [1::'test'::<ROOT>] [MessageRejected] [PacketLength: 5] Encountered an error:
                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid ConfirmHandshakeResponse with payload 1 from client [1] 'test'. Encountered 1 error(s):
                    1. Expected endianness verification payload to be 0102fdfe but found 00000001.
                    """ );

            logs.GetAllClient()
                .Skip( 9 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::'test'::<ROOT>] [Disposing]",
                    "[1::'test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientFailsToSendHandshakeRequestInTime()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromTicks( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
            },
            client );

        await clientTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAllClient()
                .Skip( 1 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [WaitingForMessage] Operation cancelled",
                    "[1::<ROOT>] [Disposing]",
                    "[1::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task DisposalAfterEstablishingHandshake_ShouldBeHandledCorrectly()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                c.SendConfirmHandshakeResponse();
            },
            client );

        await clientTask;
        await Task.Delay( 15 );
        server.Clients.TryGetById( 1 )?.Dispose();
        await endSource.Task;

        using ( new AssertionScope() )
        {
            server.Clients.Count.Should().Be( 0 );

            logs.GetAllClient()
                .Skip( 1 )
                .SkipLast( 2 )
                .Should()
                .BeSequentiallyEqualTo(
                    "[1::<ROOT>] [WaitingForMessage]",
                    "[1::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeRequest",
                    "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 1 second(s), PingInterval = 10 second(s))",
                    "[1::'test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeAcceptedResponse",
                    "[1::'test'::<ROOT>] [WaitingForMessage]",
                    "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 5] ConfirmHandshakeResponse",
                    "[1::'test'::<ROOT>] [WaitingForMessage]",
                    "[1::'test'::<ROOT>] [Disposing]" );
        }
    }

    private static void AssertClientData(byte[][] received, params (int Length, MessageBrokerClientEndpoint Endpoint)[] expected)
    {
        received.Should().HaveCount( expected.Length );
        for ( var i = 0; i < expected.Length; ++i )
        {
            received.ElementAtOrDefault( i ).Should().HaveCount( expected[i].Length );
            (received.ElementAtOrDefault( i )?.ElementAtOrDefault( 0 )).Should().Be( ( byte )expected[i].Endpoint );
        }
    }
}
