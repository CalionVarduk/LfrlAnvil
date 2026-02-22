using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Functional;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests
{
    public sealed class Ack : TestsBase, IClassFixture<SharedResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public Ack(SharedResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
        }

        [Fact]
        public async Task Ack_ShouldBeSent_WhenInvokedThroughCallbackArgs()
        {
            var endSource = new SafeTaskCompletionSource();
            var ackResultSource = new SafeTaskCompletionSource<Result<bool>>();
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
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
                s.SendMessageNotification( 1, 1, 2, 1, 3, [ 1, 2, 3 ] );
                s.ReadMessageNotificationAck();
            } );

            await client.Listeners.BindAsync( "foo", async (a, _) => ackResultSource.Complete( await a.AckAsync() ) );
            await serverTask;
            await endSource.Task;
            var ackResult = await ackResultSource.Task;

            Assertion.All(
                    ackResult.Exception.TestNull(),
                    ackResult.Value.TestTrue(),
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 48)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 2, AckId = 1, StreamId = 3, MessageId = 1, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 3",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', StreamId = 3, MessageId = 1, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 3 (start)",
                                "[AcknowledgingMessage] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 3, MessageId = 1, Retry = 0, Redelivery = 0, MessageTraceId = 2",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (MessageNotificationAck, Length = 33)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (MessageNotificationAck, Length = 33)",
                                "[MessageAcknowledged] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 3, MessageId = 1, Retry = 0, Redelivery = 0, IsNack = False",
                                "[Trace:Ack] Client = [1] 'test', TraceId = 3 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 48)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SendMessageAckAsync_ShouldSendAck()
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
                s.ReadMessageNotificationAck();
            } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.SetRetryPolicy( 1 ).SetMaxRedeliveries( 1 ) );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            var ackResult = Result.Error<bool>( new Exception() );
            if ( listener is not null )
                ackResult = await listener.SendMessageAckAsync( 1, 2, 3, 1, 1 );

            await serverTask;

            Assertion.All(
                    ackResult.Exception.TestNull(),
                    ackResult.Value.TestTrue(),
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 2 (start)",
                                "[AcknowledgingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 2, MessageId = 3, Retry = 1, Redelivery = 1",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationAck, Length = 33)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationAck, Length = 33)",
                                "[MessageAcknowledged] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 2, MessageId = 3, Retry = 1, Redelivery = 1, IsNack = False",
                                "[Trace:Ack] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Ack_ShouldThrowArgumentOutOfRangeException_WhenInvokedThroughCallbackArgsForMessageFromDeadLetter()
        {
            Exception? exception = null;
            var endSource = new SafeTaskCompletionSource();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        MessageBrokerClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerClientTraceEventType.MessageNotification )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
                s.SendMessageNotification( -1, 1, 2, 1, 3, [ 1, 2, 3 ] );
            } );

            await client.Listeners.BindAsync( "foo", async (a, _) => await a.AckAsync() );
            await serverTask;
            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<ArgumentOutOfRangeException>(),
                    client.State.TestEquals( MessageBrokerClientState.Running ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldThrowArgumentOutOfRangeException_WhenInvokedThroughCallbackArgsForMessageFromDeadLetter()
        {
            Exception? exception = null;
            var endSource = new SafeTaskCompletionSource();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        MessageBrokerClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerClientTraceEventType.MessageNotification )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
                s.SendMessageNotification( -1, 1, 2, 1, 3, [ 1, 2, 3 ] );
            } );

            await client.Listeners.BindAsync( "foo", async (a, _) => await a.NegativeAckAsync() );
            await serverTask;
            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<ArgumentOutOfRangeException>(),
                    client.State.TestEquals( MessageBrokerClientState.Running ) )
                .Go();
        }

        [Fact]
        public async Task SendMessageAckAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
            } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;
            var listener = client.Listeners.TryGetByChannelId( 1 );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                if ( listener is not null )
                    await listener.SendMessageAckAsync( 1, 1, 0, 0, 0 );
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task SendMessageAckAsync_ShouldNotSendAck_WhenListenerIsNotBound()
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
            } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            var listener = client.Listeners.TryGetByChannelId( 1 );
            serverTask = server.GetTask( s =>
            {
                s.ReadUnbindListenerRequest();
                s.SendListenerUnboundResponse( false, false );
            } );

            if ( listener is not null )
                await listener.UnbindAsync();

            await serverTask;

            var ackResult = Result.Error<bool>( new Exception() );
            if ( listener is not null )
                ackResult = await listener.SendMessageAckAsync( 1, 1, 0, 0, 0 );

            Assertion.All( ackResult.Exception.TestNull(), ackResult.Value.TestFalse() ).Go();
        }

        [Fact]
        public async Task SendMessageAckAsync_ShouldThrowMessageBrokerClientMessageException_WhenAcksAreDisabled()
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
            } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.EnableAcks( false ) );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            await serverTask;

            var action = Lambda.Of( () => listener?.SendMessageAckAsync( 1, 1, 0, 0, 0 ) ?? ValueTask.FromResult( Result.Create( true ) ) );

            action.Test( exc => exc.TestType()
                    .Exact<MessageBrokerClientMessageException>( e => Assertion.All(
                        e.Client.TestRefEquals( client ),
                        e.Listener.TestRefEquals( listener ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( 0, 0, 0, 0 )]
        [InlineData( 1, 0, 0, 0 )]
        [InlineData( 1, 0, 2, 0 )]
        [InlineData( 1, 0, 0, 3 )]
        public async Task SendMessageAckAsync_ShouldThrowArgumentOutOfRangeException_WhenSomeParametersAreInvalid(
            int ackId,
            int streamId,
            int retryAttempt,
            int redeliveryAttempt)
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
            } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.SetRetryPolicy( 1 ).SetMaxRedeliveries( 2 ) );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            await serverTask;

            var action = Lambda.Of( () => listener?.SendMessageAckAsync( ackId, streamId, 0, retryAttempt, redeliveryAttempt )
                ?? ValueTask.FromResult( Result.Create( true ) ) );

            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldBeSent_WhenInvokedThroughCallbackArgs()
        {
            var endSource = new SafeTaskCompletionSource();
            var nackResultSource = new SafeTaskCompletionSource<Result<bool>>();
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
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
                s.SendMessageNotification( 1, 1, 2, 1, 3, [ 1, 2, 3 ] );
                s.ReadMessageNotificationNegativeAck();
            } );

            await client.Listeners.BindAsync( "foo", async (a, _) => nackResultSource.Complete( await a.NegativeAckAsync() ) );
            await serverTask;
            await endSource.Task;
            var nackResult = await nackResultSource.Task;

            Assertion.All(
                    nackResult.Exception.TestNull(),
                    nackResult.Value.TestTrue(),
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 48)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 2, AckId = 1, StreamId = 3, MessageId = 1, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 3",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', StreamId = 3, MessageId = 1, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 3 (start)",
                                "[AcknowledgingMessage] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 3, MessageId = 1, Retry = 0, Redelivery = 0, MessageTraceId = 2, NACK = (SkipRetry = False, SkipDeadLetter = False, IsAutomatic = False)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (MessageNotificationNack, Length = 38)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (MessageNotificationNack, Length = 38)",
                                "[MessageAcknowledged] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 3, MessageId = 1, Retry = 0, Redelivery = 0, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 3 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 48)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SendNegativeMessageAckAsync_ShouldSendNegativeAck_WithNoRetry()
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
                s.ReadMessageNotificationNegativeAck();
            } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.SetRetryPolicy( 1 ).SetMaxRedeliveries( 1 ) );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            var ackResult = Result.Error<bool>( new Exception() );
            if ( listener is not null )
                ackResult = await listener.SendNegativeMessageAckAsync( 1, 2, 3, 1, 1, MessageBrokerNegativeAck.NoRetry() );

            await serverTask;

            Assertion.All(
                    ackResult.Exception.TestNull(),
                    ackResult.Value.TestTrue(),
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 2 (start)",
                                "[AcknowledgingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 2, MessageId = 3, Retry = 1, Redelivery = 1, NACK = (SkipRetry = True, SkipDeadLetter = False, IsAutomatic = False)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationNack, Length = 38)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationNack, Length = 38)",
                                "[MessageAcknowledged] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 2, MessageId = 3, Retry = 1, Redelivery = 1, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SendNegativeMessageAckAsync_ShouldSendNegativeAck_WithNoDeadLetter()
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
                s.ReadMessageNotificationNegativeAck();
            } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.SetRetryPolicy( 1 ).SetMaxRedeliveries( 1 ) );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            var ackResult = Result.Error<bool>( new Exception() );
            if ( listener is not null )
                ackResult = await listener.SendNegativeMessageAckAsync( 1, 2, 3, 1, 1, MessageBrokerNegativeAck.NoDeadLetter() );

            await serverTask;

            Assertion.All(
                    ackResult.Exception.TestNull(),
                    ackResult.Value.TestTrue(),
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 2 (start)",
                                "[AcknowledgingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 2, MessageId = 3, Retry = 1, Redelivery = 1, NACK = (SkipRetry = False, SkipDeadLetter = True, IsAutomatic = False)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationNack, Length = 38)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationNack, Length = 38)",
                                "[MessageAcknowledged] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 2, MessageId = 3, Retry = 1, Redelivery = 1, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SendNegativeMessageAckAsync_ShouldSendNegativeAck_WithCustomDelay()
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
                s.ReadMessageNotificationNegativeAck();
            } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.SetRetryPolicy( 2 ).SetMaxRedeliveries( 2 ) );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            var ackResult = Result.Error<bool>( new Exception() );
            if ( listener is not null )
                ackResult = await listener.SendNegativeMessageAckAsync(
                    1,
                    2,
                    3,
                    2,
                    2,
                    MessageBrokerNegativeAck.Retry( Duration.FromSeconds( 3 ) ) );

            await serverTask;

            Assertion.All(
                    ackResult.Exception.TestNull(),
                    ackResult.Value.TestTrue(),
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 2 (start)",
                                "[AcknowledgingMessage] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 2, MessageId = 3, Retry = 2, Redelivery = 2, NACK = (SkipRetry = False, SkipDeadLetter = False, RetryDelay = 3 second(s), IsAutomatic = False)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationNack, Length = 38)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationNack, Length = 38)",
                                "[MessageAcknowledged] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 2, MessageId = 3, Retry = 2, Redelivery = 2, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SendNegativeMessageAckAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
            } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;
            var listener = client.Listeners.TryGetByChannelId( 1 );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                if ( listener is not null )
                    await listener.SendNegativeMessageAckAsync( 1, 1, 0, 0, 0 );
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task SendNegativeMessageAckAsync_ShouldNotSendAck_WhenListenerIsNotBound()
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
            } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            var listener = client.Listeners.TryGetByChannelId( 1 );
            serverTask = server.GetTask( s =>
            {
                s.ReadUnbindListenerRequest();
                s.SendListenerUnboundResponse( false, false );
            } );

            if ( listener is not null )
                await listener.UnbindAsync();

            await serverTask;

            var ackResult = Result.Error<bool>( new Exception() );
            if ( listener is not null )
                ackResult = await listener.SendNegativeMessageAckAsync( 1, 1, 0, 0, 0 );

            Assertion.All( ackResult.Exception.TestNull(), ackResult.Value.TestFalse() ).Go();
        }

        [Fact]
        public async Task SendNegativeMessageAckAsync_ShouldThrowMessageBrokerClientMessageException_WhenAcksAreDisabled()
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
            } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.EnableAcks( false ) );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            await serverTask;

            var action = Lambda.Of( () =>
                listener?.SendNegativeMessageAckAsync( 1, 1, 0, 0, 0 ) ?? ValueTask.FromResult( Result.Create( true ) ) );

            action.Test( exc => exc.TestType()
                    .Exact<MessageBrokerClientMessageException>( e => Assertion.All(
                        e.Client.TestRefEquals( client ),
                        e.Listener.TestRefEquals( listener ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( 0, 0, 0, 0 )]
        [InlineData( 1, 0, 0, 0 )]
        [InlineData( 1, 0, 2, 0 )]
        [InlineData( 1, 0, 0, 3 )]
        public async Task SendNegativeMessageAckAsync_ShouldThrowArgumentOutOfRangeException_WhenSomeParametersAreInvalid(
            int ackId,
            int streamId,
            int retryAttempt,
            int redeliveryAttempt)
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
            var serverTask = server.GetTask( s =>
            {
                s.Read( GetBindListenerRequest( "foo" ) );
                s.SendListenerBoundResponse( true, true, 1, 1 );
            } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.SetRetryPolicy( 1 ).SetMaxRedeliveries( 2 ) );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            await serverTask;

            var action = Lambda.Of( () => listener?.SendNegativeMessageAckAsync( ackId, streamId, 0, retryAttempt, redeliveryAttempt )
                ?? ValueTask.FromResult( Result.Create( true ) ) );

            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Pure]
        private static Protocol.BindListenerRequest GetBindListenerRequest(
            string channelName,
            string? queueName = null,
            short prefetchHint = 1,
            int maxRetries = 0,
            Duration retryDelay = default,
            int maxRedeliveries = 0,
            Duration? minAckTimeout = null,
            int deadLetterCapacity = 0,
            Duration deadLetterRetention = default,
            string? filterExpression = null,
            bool createChannelIfNotExists = true,
            bool isEphemeral = true)
        {
            return new Protocol.BindListenerRequest(
                channelName,
                queueName,
                prefetchHint,
                maxRetries,
                retryDelay,
                maxRedeliveries,
                minAckTimeout ?? MessageBrokerListenerOptions.DefaultMinAckTimeout,
                deadLetterCapacity,
                deadLetterRetention,
                filterExpression,
                createChannelIfNotExists,
                isEphemeral );
        }
    }
}
