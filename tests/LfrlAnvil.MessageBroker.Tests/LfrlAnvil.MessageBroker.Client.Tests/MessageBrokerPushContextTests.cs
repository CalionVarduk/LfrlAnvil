using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Functional;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerPushContextTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageBrokerPushContextTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task GetPushContext_ShouldReturnContextWhichAllowsToPushSingleMessage()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var data = new byte[] { 1, 2, 3, 4, 5 };
        serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.PushMessageHeader( 1, data.Length, true ) );
                s.SendMessageAcceptedResponse( 1 );
            } );

        var remainingRoutingPacketLength = MemorySize.Zero;
        var remainingPacketLength = MemorySize.Zero;
        var result = Result.Create( MessageBrokerPushResult.CreateNotBound( true ) );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            using var ctx = publisher.GetPushContext();
            remainingRoutingPacketLength = ctx.RemainingRoutingPacketLength;
            remainingPacketLength = ctx.RemainingPacketLength;
            result = await ctx.Append( data ).PushAsync();
        }

        await serverTask;

        Assertion.All(
                remainingRoutingPacketLength.TestEquals( client.MaxNetworkPacketLength ),
                remainingPacketLength.TestEquals(
                    client.MaxNetworkMessagePacketLength
                    - MemorySize.FromBytes( Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Length ) ),
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.Confirm.TestTrue(),
                result.Value.Id.TestEquals( 1UL ),
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, Confirm = True",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, MessageId = 1",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task GetPushContext_ShouldReturnContextWhichAllowsToPushSingleMessage_UsingBufferWriter()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var data = new byte[] { 1, 2, 3, 4, 5 };
        serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.PushMessageHeader( 1, data.Length, true ) );
                s.SendMessageAcceptedResponse( 2 );
            } );

        var remainingRoutingPacketLength = MemorySize.Zero;
        var remainingPacketLength = MemorySize.Zero;
        var result = Result.Create( MessageBrokerPushResult.CreateNotBound( true ) );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            using var ctx = publisher.GetPushContext();
            data.AsSpan( 0, 2 ).CopyTo( ctx.GetSpan( 2 ) );
            ctx.Advance( 2 );
            data.AsMemory( 2 ).CopyTo( ctx.GetMemory( 3 ) );
            ctx.Advance( 3 );
            remainingRoutingPacketLength = ctx.RemainingRoutingPacketLength;
            remainingPacketLength = ctx.RemainingPacketLength;
            result = await ctx.PushAsync();
        }

        await serverTask;

        Assertion.All(
                remainingRoutingPacketLength.TestEquals( client.MaxNetworkPacketLength ),
                remainingPacketLength.TestEquals(
                    client.MaxNetworkMessagePacketLength
                    - MemorySize.FromBytes( Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Length + data.Length ) ),
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.Confirm.TestTrue(),
                result.Value.Id.TestEquals( 2UL ),
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, Confirm = True",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, MessageId = 2",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task GetPushContext_ShouldReturnContextWhichAllowsToPushSingleMessage_WhenMessageIsLargerThanInitialCapacity()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var data = new byte[2048];
        serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.PushMessageHeader( 1, data.Length, true ) );
                s.SendMessageAcceptedResponse( 3 );
            } );

        var result = Result.Create( MessageBrokerPushResult.CreateNotBound( true ) );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            using var ctx = publisher.GetPushContext();
            result = await ctx.Append( data ).PushAsync();
        }

        await serverTask;

        Assertion.All(
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.Confirm.TestTrue(),
                result.Value.Id.TestEquals( 3UL ),
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 2048, Confirm = True",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 2058)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 2058)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 2048, MessageId = 3",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task GetPushContext_ShouldReturnContextWhichAllowsToPushSingleMessage_WithoutConfirmation()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var data = new byte[] { 1, 2, 3, 4, 5 };
        serverTask = server.GetTask( s => s.Read( new Protocol.PushMessageHeader( 1, data.Length, false ) ) );

        var result = Result.Create( MessageBrokerPushResult.CreateNotBound( false ) );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            using var ctx = publisher.GetPushContext();
            result = await ctx.Append( data ).PushAsync( confirm: false );
        }

        await serverTask;

        Assertion.All(
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.Confirm.TestFalse(),
                result.Value.Id.TestNull(),
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, Confirm = False",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task GetPushContext_ShouldReturnContextThatThrowsObjectDisposedExceptionWhenActedOnAfterDisposal()
    {
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var remainingRoutingPacketLengthAction = Lambda.Of( () => { } );
        var remainingPacketLength = Lambda.Of( () => { } );
        var getMemoryAction = Lambda.Of( () => { } );
        var getSpanAction = Lambda.Of( () => { } );
        var advanceAction = Lambda.Of( () => { } );
        var appendAction = Lambda.Of( () => { } );
        var pushAction = Lambda.Of( () => Task.CompletedTask );
        var enqueueAction = Lambda.Of( () => Task.CompletedTask );
        var addIdTargetAction = Lambda.Of( () => { } );
        var addNameTargetAction = Lambda.Of( () => { } );
        var disposeAction = Lambda.Of( () => throw new Exception() );

        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            var ctx = publisher.GetPushContext();
            ctx.Dispose();
            remainingRoutingPacketLengthAction = Lambda.Of( () => { _ = ctx.RemainingRoutingPacketLength; } );
            remainingPacketLength = Lambda.Of( () => { _ = ctx.RemainingPacketLength; } );
            getMemoryAction = Lambda.Of( () => { _ = ctx.GetMemory( 5 ); } );
            getSpanAction = Lambda.Of( () => { _ = ctx.GetSpan( 5 ); } );
            advanceAction = Lambda.Of( () => ctx.Advance( 5 ) );
            appendAction = Lambda.Of( () => { _ = ctx.Append( Array.Empty<byte>() ); } );
            pushAction = Lambda.Of( async () => await ctx.PushAsync() );
            enqueueAction = Lambda.Of( async () => await ctx.EnqueueAsync() );
            addIdTargetAction = Lambda.Of( () => { _ = ctx.AddTarget( 1 ); } );
            addNameTargetAction = Lambda.Of( () => { _ = ctx.AddTarget( "foo" ); } );
            disposeAction = Lambda.Of( () => ctx.Dispose() );
        }

        Assertion.All(
                remainingRoutingPacketLengthAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                remainingPacketLength.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                getMemoryAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                getSpanAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                advanceAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                appendAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                pushAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                enqueueAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                addIdTargetAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                addNameTargetAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                disposeAction.Test( exc => exc.TestNull() ) )
            .Go();
    }

    [Fact]
    public async Task GetPushContext_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
    {
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var publisher = client.Publishers.TryGetByChannelId( 1 );
        await client.DisposeAsync();
        var action = publisher is not null ? Lambda.Of( () => { _ = publisher.GetPushContext(); } ) : Lambda.Of( () => { } );

        action.Test( exc => exc.TestType().Exact<MessageBrokerClientDisposedException>() ).Go();
    }

    [Fact]
    public async Task GetPushContext_ShouldReturnContextWhichAllowsToPushSingleMessage_WithIdTargets()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var data = new byte[] { 1, 2, 3, 4, 5 };
        serverTask = server.GetTask(
            s =>
            {
                s.ReadPushMessageRouting( 10 );
                s.Read( new Protocol.PushMessageHeader( 1, data.Length, true ) );
                s.SendMessageAcceptedResponse( 1 );
            } );

        var result = Result.Create( MessageBrokerPushResult.CreateNotBound( true ) );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            using var ctx = publisher.GetPushContext();
            result = await ctx.AddTarget( 1 ).AddTarget( 2 ).Append( data ).PushAsync();
        }

        await serverTask;

        Assertion.All(
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.Confirm.TestTrue(),
                result.Value.Id.TestEquals( 1UL ),
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, RoutingTargetCount = 2, Confirm = True",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessageRouting, Length = 17)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessageRouting, Length = 17)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, MessageId = 1",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task GetPushContext_ShouldReturnContextWhichAllowsToPushSingleMessage_WithNameTargets()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var data = new byte[] { 1, 2, 3, 4, 5 };
        serverTask = server.GetTask(
            s =>
            {
                s.ReadPushMessageRouting( 76 );
                s.Read( new Protocol.PushMessageHeader( 1, data.Length, true ) );
                s.SendMessageAcceptedResponse( 1 );
            } );

        var result = Result.Create( MessageBrokerPushResult.CreateNotBound( true ) );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            using var ctx = publisher.GetPushContext();
            result = await ctx
                .AddTarget( "long-target-client-name" )
                .AddTarget( "even-longer-client-name-to-test-capacity-increase" )
                .Append( data )
                .PushAsync();
        }

        await serverTask;

        Assertion.All(
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.Confirm.TestTrue(),
                result.Value.Id.TestEquals( 1UL ),
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, RoutingTargetCount = 2, Confirm = True",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessageRouting, Length = 83)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessageRouting, Length = 83)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, MessageId = 1",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task GetPushContext_ShouldReturnContextWhichAllowsToPushSingleMessage_WithTargetsAndWithoutConfirmation()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var data = new byte[] { 1, 2, 3, 4, 5 };
        serverTask = server.GetTask(
            s =>
            {
                s.ReadPushMessageRouting( 10 );
                s.Read( new Protocol.PushMessageHeader( 1, data.Length, false ) );
                s.SendMessageAcceptedResponse( 1 );
            } );

        var result = Result.Create( MessageBrokerPushResult.CreateNotBound( true ) );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            using var ctx = publisher.GetPushContext();
            result = await ctx.AddTarget( 1 ).AddTarget( "foo" ).Append( data ).PushAsync( confirm: false );
        }

        await serverTask;

        Assertion.All(
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.Confirm.TestFalse(),
                result.Value.Id.TestNull(),
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, RoutingTargetCount = 2, Confirm = False",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessageRouting, Length = 17)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessageRouting, Length = 17)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 15)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task GetPushContext_ShouldFailToPushMessage_WhenNetworkPacketLengthsAreExceeded()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client, maxNetworkMessagePacketLength: MemorySize.FromKilobytes( 20 ) );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var remainingRoutingPacketLength = MemorySize.Zero;
        var remainingPacketLength = MemorySize.Zero;
        var result = Result.Create( MessageBrokerPushResult.CreateNotBound( true ) );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        if ( publisher is not null )
        {
            using var ctx = publisher
                .GetPushContext( MemorySize.FromKilobytes( 21 ) )
                .Append(
                    new byte[( int )MemorySize.FromKilobytes( 20 ).Bytes
                        - Protocol.PacketHeader.Length
                        - Protocol.MessageNotificationHeader.Length
                        + 1] );

            for ( var i = 0; i < 31; ++i )
                ctx.AddTarget( new string( 'x', 512 ) );

            ctx.AddTarget( new string( 'x', 442 ) );
            remainingRoutingPacketLength = ctx.RemainingRoutingPacketLength;
            remainingPacketLength = ctx.RemainingPacketLength;
            result = await ctx.PushAsync();
        }

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Running ),
                remainingRoutingPacketLength.TestEquals( MemorySize.FromBytes( -1 ) ),
                remainingPacketLength.TestEquals( MemorySize.FromBytes( -1 ) ),
                result.Exception.TestType().Exact<InvalidOperationException>(),
                result.Value.NotBound.TestTrue(),
                result.Value.Confirm.TestFalse(),
                result.Value.Id.TestNull(),
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 20436, RoutingTargetCount = 32, Confirm = True",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            System.InvalidOperationException: Message could not be pushed to the server. Encountered 2 error(s):
                            1. Max network message packet length of 20445 B has been exceeded by 1 B.
                            2. Max network packet length of 16384 B for message routing has been exceeded by 1 B.
                            """,
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task AddTarget_WithId_ShouldThrowArgumentOutOfRangeException_WhenIdIsLessThanOne()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var publisher = client.Publishers.TryGetByChannelId( 1 );
        Assume.IsNotNull( publisher );
        using var ctx = publisher.GetPushContext();
        var action = Lambda.Of( () => ctx.AddTarget( 0 ) );

        await serverTask;

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 513 )]
    public async Task AddTarget_WithName_ShouldThrowArgumentOutOfRangeException_WhenNameLengthIsInvalid(int length)
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var publisher = client.Publishers.TryGetByChannelId( 1 );
        Assume.IsNotNull( publisher );
        using var ctx = publisher.GetPushContext();
        var name = new string( 'x', length );
        var action = Lambda.Of( () => ctx.AddTarget( name ) );

        await serverTask;

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public async Task AddTarget_WithId_ShouldThrowInvalidOperationException_WhenTargetCountLimitIsReached()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var publisher = client.Publishers.TryGetByChannelId( 1 );
        Assume.IsNotNull( publisher );
        using var ctx = publisher.GetPushContext();
        for ( var i = 0; i < short.MaxValue; ++i )
            ctx.AddTarget( 1 );

        var action = Lambda.Of( () => ctx.AddTarget( 1 ) );

        await serverTask;

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task AddTarget_WithName_ShouldThrowInvalidOperationException_WhenTargetCountLimitIsReached()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        var publisher = client.Publishers.TryGetByChannelId( 1 );
        Assume.IsNotNull( publisher );
        using var ctx = publisher.GetPushContext();
        for ( var i = 0; i < short.MaxValue; ++i )
            ctx.AddTarget( 1 );

        var action = Lambda.Of( () => ctx.AddTarget( "foo" ) );

        await serverTask;

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public async Task EnqueueAsyncWithFinalizerPushAsync_ShouldAllowToEasilyPushMessagesInBatch()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        await server.EstablishHandshake( client, maxBatchPacketCount: 10, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
        var serverTask = server.GetTask(
            s =>
            {
                s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                s.SendPublisherBoundResponse( true, true, 1, 1 );
            } );

        await client.Publishers.BindAsync( "foo" );
        await serverTask;

        serverTask = server.GetTask(
            s =>
            {
                s.ReadBatch(
                [
                    (MessageBrokerServerEndpoint.PushMessage, Protocol.PushMessageHeader.Length + 1),
                    (MessageBrokerServerEndpoint.PushMessageRouting, Protocol.PushMessageRoutingHeader.Length + 5),
                    (MessageBrokerServerEndpoint.PushMessage, Protocol.PushMessageHeader.Length + 2),
                    (MessageBrokerServerEndpoint.PushMessage, Protocol.PushMessageHeader.Length + 3),
                    (MessageBrokerServerEndpoint.PushMessageRouting, Protocol.PushMessageRoutingHeader.Length + 5),
                    (MessageBrokerServerEndpoint.PushMessage, Protocol.PushMessageHeader.Length + 4)
                ] );

                s.SendMessageAcceptedResponse( 2 );
                s.SendMessageAcceptedResponse( 3 );
            } );

        var publisher = client.Publishers.TryGetByChannelId( 1 )!;
        var contexts = Enumerable.Range( 0, 4 ).Select( _ => publisher.GetPushContext() ).ToArray();

        contexts[0].Append( [ 1 ] );
        contexts[1].Append( [ 2, 3 ] ).AddTarget( 2 );
        contexts[2].Append( [ 4, 5, 6 ] );
        contexts[3].Append( [ 7, 8, 9, 10 ] ).AddTarget( "foo" );

        var finalizers = new MessageBrokerPushMessageFinalizer[contexts.Length];
        for ( var i = 0; i < contexts.Length; ++i )
            finalizers[i] = (await contexts[i].EnqueueAsync( confirm: i > 1 )).GetValueOrThrow();

        var result = new Result<MessageBrokerPushResult>[finalizers.Length];
        for ( var i = 0; i < finalizers.Length; ++i )
            result[i] = await finalizers[i].PushAsync();

        await serverTask;
        foreach ( var c in contexts )
            c.Dispose();

        Assertion.All(
                result[0].Exception.TestNull(),
                result[0].Value.NotBound.TestFalse(),
                result[0].Value.Confirm.TestFalse(),
                result[0].Value.Id.TestNull(),
                result[1].Exception.TestNull(),
                result[1].Value.NotBound.TestFalse(),
                result[1].Value.Confirm.TestFalse(),
                result[1].Value.Id.TestNull(),
                result[2].Exception.TestNull(),
                result[2].Value.NotBound.TestFalse(),
                result[2].Value.Confirm.TestTrue(),
                result[2].Value.Id.TestEquals( 2UL ),
                result[3].Exception.TestNull(),
                result[3].Value.NotBound.TestFalse(),
                result[3].Value.Confirm.TestTrue(),
                result[3].Value.Id.TestEquals( 3UL ),
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 1, Confirm = False",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (Batch, Length = 81), PacketCount = 6",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (Batch, Length = 81)",
                            "[SendPacket:Batched] Client = [1] 'test', TraceId = 2, BatchTraceId = 2, Packet = (PushMessage, Length = 11)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 1",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 3 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [1] 'foo', Length = 2, RoutingTargetCount = 1, Confirm = False",
                            "[SendPacket:Batched] Client = [1] 'test', TraceId = 3, BatchTraceId = 2, Packet = (PushMessageRouting, Length = 12)",
                            "[SendPacket:Batched] Client = [1] 'test', TraceId = 3, BatchTraceId = 2, Packet = (PushMessage, Length = 12)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [1] 'foo', Length = 2",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 3 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo', Stream = [1] 'foo', Length = 3, Confirm = True",
                            "[SendPacket:Batched] Client = [1] 'test', TraceId = 4, BatchTraceId = 2, Packet = (PushMessage, Length = 13)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo', Stream = [1] 'foo', Length = 3, MessageId = 2",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 5 (start)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 5, Channel = [1] 'foo', Stream = [1] 'foo', Length = 4, RoutingTargetCount = 1, Confirm = True",
                            "[SendPacket:Batched] Client = [1] 'test', TraceId = 5, BatchTraceId = 2, Packet = (PushMessageRouting, Length = 12)",
                            "[SendPacket:Batched] Client = [1] 'test', TraceId = 5, BatchTraceId = 2, Packet = (PushMessage, Length = 14)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 5, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 5, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 5, Channel = [1] 'foo', Stream = [1] 'foo', Length = 4, MessageId = 3",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 5 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                    ] ) )
            .Go();
    }
}
