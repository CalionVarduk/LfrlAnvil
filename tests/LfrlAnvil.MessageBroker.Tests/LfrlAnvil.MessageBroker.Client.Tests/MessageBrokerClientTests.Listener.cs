using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading;
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
    public class Listener : TestsBase, IClassFixture<SharedResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public Listener(SharedResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
        }

        [Theory]
        [InlineData( true, true )]
        [InlineData( true, false )]
        [InlineData( false, true )]
        [InlineData( false, false )]
        public async Task BindAsync_ShouldCreateListenerCorrectly(bool channelCreated, bool queueCreated)
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();
            MessageBrokerListenerCallback callback = (_, _) => ValueTask.CompletedTask;

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var queueName = "bar";
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( channelName, queueName: queueName ) );
                    s.SendListenerBoundResponse( channelCreated, queueCreated, 1, 2 );
                } );

            var result = await client.Listeners.BindAsync( channelName, callback, queueName: queueName );
            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.AlreadyBound.TestFalse(),
                            r.ChannelCreated.TestEquals( channelCreated ),
                            r.QueueCreated.TestEquals( queueCreated ),
                            r.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                            r.ToString()
                                .TestEquals(
                                    channelCreated
                                        ? queueCreated
                                            ? $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound) (channel created) (queue created)"
                                            : $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound) (channel created)"
                                        : queueCreated
                                            ? $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound) (queue created)"
                                            : $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound)" ) ) ),
                    client.Listeners.Count.TestEquals( 1 ),
                    client.Listeners.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ) ] ),
                    client.Listeners.TryGetByChannelName( channelName ).TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                    client.Listeners.TryGetByChannelId( 1 )
                        .TestNotNull(
                            listener => Assertion.All(
                                "listener",
                                listener.Client.TestRefEquals( client ),
                                listener.ChannelId.TestEquals( 1 ),
                                listener.ChannelName.TestEquals( channelName ),
                                listener.QueueId.TestEquals( 2 ),
                                listener.QueueName.TestEquals( queueName ),
                                listener.PrefetchHint.TestEquals( ( short )1 ),
                                listener.MaxRetries.TestEquals( 0 ),
                                listener.RetryDelay.TestEquals( Duration.Zero ),
                                listener.MaxRedeliveries.TestEquals( 0 ),
                                listener.MinAckTimeout.TestEquals( MessageBrokerListenerOptions.DefaultMinAckTimeout ),
                                listener.AreAcksEnabled.TestTrue(),
                                listener.Callback.TestRefEquals( callback ),
                                listener.State.TestEquals( MessageBrokerListenerState.Bound ),
                                listener.ToString()
                                    .TestEquals(
                                        $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound)" ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                                $"[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = '{channelName}', QueueName = '{queueName}', PrefetchHint = 1, MaxRetries = 0, MaxRedeliveries = 0, MinAckTimeout = 600 second(s), CreateChannelIfNotExists = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 32)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 32)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 14)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 14)",
                                $"[ListenerBound] Client = [1] 'test', TraceId = 1, Channel = [1] '{channelName}'{(channelCreated ? " (created)" : string.Empty)}, Queue = [2] '{queueName}'{(queueCreated ? " (created)" : string.Empty)}",
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerBoundResponse, Length = 14)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldCreateListenerCorrectly_WithRetriesAndRedeliveriesEnabled()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();
            MessageBrokerListenerCallback callback = (_, _) => ValueTask.CompletedTask;

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( channelName ) );
                    s.SendListenerBoundResponse( false, false, 1, 2 );
                } );

            var result = await client.Listeners.BindAsync(
                channelName,
                callback,
                MessageBrokerListenerOptions.Default.SetRetryPolicy( 3 ).SetMaxRedeliveries( 4 ).SetPrefetchHint( 5 ) );

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.AlreadyBound.TestFalse(),
                            r.ChannelCreated.TestFalse(),
                            r.QueueCreated.TestFalse(),
                            r.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                            r.ToString()
                                .TestEquals( $"[1] 'test' => [1] '{channelName}' listener (using [2] '{channelName}' queue) (Bound)" ) ) ),
                    client.Listeners.Count.TestEquals( 1 ),
                    client.Listeners.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ) ] ),
                    client.Listeners.TryGetByChannelName( channelName ).TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                    client.Listeners.TryGetByChannelId( 1 )
                        .TestNotNull(
                            listener => Assertion.All(
                                "listener",
                                listener.Client.TestRefEquals( client ),
                                listener.ChannelId.TestEquals( 1 ),
                                listener.ChannelName.TestEquals( channelName ),
                                listener.QueueId.TestEquals( 2 ),
                                listener.QueueName.TestEquals( channelName ),
                                listener.PrefetchHint.TestEquals( ( short )5 ),
                                listener.MaxRetries.TestEquals( 3 ),
                                listener.RetryDelay.TestEquals( MessageBrokerListenerOptions.DefaultRetryDelay ),
                                listener.MaxRedeliveries.TestEquals( 4 ),
                                listener.MinAckTimeout.TestEquals( MessageBrokerListenerOptions.DefaultMinAckTimeout ),
                                listener.AreAcksEnabled.TestTrue(),
                                listener.Callback.TestRefEquals( callback ),
                                listener.State.TestEquals( MessageBrokerListenerState.Bound ),
                                listener.ToString()
                                    .TestEquals(
                                        $"[1] 'test' => [1] '{channelName}' listener (using [2] '{channelName}' queue) (Bound)" ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                                $"[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', QueueName = 'foo', PrefetchHint = 5, MaxRetries = 3, RetryDelay = 30 second(s), MaxRedeliveries = 4, MinAckTimeout = 600 second(s), CreateChannelIfNotExists = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 14)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 14)",
                                $"[ListenerBound] Client = [1] 'test', TraceId = 1, Channel = [1] '{channelName}', Queue = [2] '{channelName}'",
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerBoundResponse, Length = 14)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldCreateListenerCorrectly_WithAcksDisabled()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();
            MessageBrokerListenerCallback callback = (_, _) => ValueTask.CompletedTask;

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( channelName ) );
                    s.SendListenerBoundResponse( false, false, 1, 2 );
                } );

            var result = await client.Listeners.BindAsync(
                channelName,
                callback,
                MessageBrokerListenerOptions.Default.EnableAcks( false ) );

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.AlreadyBound.TestFalse(),
                            r.ChannelCreated.TestFalse(),
                            r.QueueCreated.TestFalse(),
                            r.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                            r.ToString()
                                .TestEquals( $"[1] 'test' => [1] '{channelName}' listener (using [2] '{channelName}' queue) (Bound)" ) ) ),
                    client.Listeners.Count.TestEquals( 1 ),
                    client.Listeners.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ) ] ),
                    client.Listeners.TryGetByChannelName( channelName ).TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                    client.Listeners.TryGetByChannelId( 1 )
                        .TestNotNull(
                            listener => Assertion.All(
                                "listener",
                                listener.Client.TestRefEquals( client ),
                                listener.ChannelId.TestEquals( 1 ),
                                listener.ChannelName.TestEquals( channelName ),
                                listener.QueueId.TestEquals( 2 ),
                                listener.QueueName.TestEquals( channelName ),
                                listener.PrefetchHint.TestEquals( ( short )1 ),
                                listener.MaxRetries.TestEquals( 0 ),
                                listener.RetryDelay.TestEquals( Duration.Zero ),
                                listener.MaxRedeliveries.TestEquals( 0 ),
                                listener.MinAckTimeout.TestEquals( Duration.Zero ),
                                listener.AreAcksEnabled.TestFalse(),
                                listener.Callback.TestRefEquals( callback ),
                                listener.State.TestEquals( MessageBrokerListenerState.Bound ),
                                listener.ToString()
                                    .TestEquals(
                                        $"[1] 'test' => [1] '{channelName}' listener (using [2] '{channelName}' queue) (Bound)" ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                                $"[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = '{channelName}', QueueName = '{channelName}', PrefetchHint = 1, MaxRetries = 0, MaxRedeliveries = 0, MinAckTimeout = <disabled>, CreateChannelIfNotExists = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 14)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 14)",
                                $"[ListenerBound] Client = [1] 'test', TraceId = 1, Channel = [1] '{channelName}', Queue = [2] '{channelName}'",
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerBoundResponse, Length = 14)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldNotThrow_WhenListenerIsAlreadyLocallyBound()
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

            var channelName = "foo";
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( channelName ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                } );

            await client.Listeners.BindAsync( channelName, (_, _) => ValueTask.CompletedTask );
            await serverTask;

            var result = await client.Listeners.BindAsync(
                channelName,
                (_, _) => ValueTask.CompletedTask,
                createChannelIfNotExists: false );

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.AlreadyBound.TestTrue(),
                            r.ChannelCreated.TestFalse(),
                            r.QueueCreated.TestFalse(),
                            r.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                            r.ToString()
                                .TestEquals(
                                    $"[1] 'test' => [1] '{channelName}' listener (using [1] '{channelName}' queue) (Bound) (already bound)" ) ) ),
                    client.Listeners.Count.TestEquals( 1 ) )
                .Go();
        }

        [Fact]
        public async Task ClientDispose_ShouldMarkAllListenersAsDisposed()
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

            var channelName = "foo";
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( channelName ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                } );

            var result = await client.Listeners.BindAsync( channelName, (_, _) => ValueTask.CompletedTask );
            await serverTask;
            await client.DisposeAsync();

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull( r => r.Listener.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                    client.Listeners.Count.TestEquals( 0 ),
                    client.Listeners.GetAll().TestEmpty() )
                .Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenChannelNameIsEmpty()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( string.Empty, (_, _) => ValueTask.CompletedTask ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenQueueNameIsNotNullAndEmpty()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask, queueName: string.Empty ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenChannelNameIsTooLong()
        {
            var name = new string( 'x', 513 );
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( name, (_, _) => ValueTask.CompletedTask ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenQueueNameIsNotNullAndTooLong()
        {
            var name = new string( 'x', 513 );
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask, queueName: name ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowMessageBrokerClientStateException_WhenClientIsNotRunning()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask ) );
            action.Test(
                    exc => exc.TestType()
                        .Exact<MessageBrokerClientStateException>(
                            e => Assertion.All(
                                e.Client.TestRefEquals( client ),
                                e.Expected.TestEquals( MessageBrokerClientState.Running ),
                                e.Actual.TestEquals( MessageBrokerClientState.Created ) ) ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerDoesNotRespondInTime()
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

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.BindListenerRequest ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', QueueName = 'foo', PrefetchHint = 1, MaxRetries = 0, MaxRedeliveries = 0, MinAckTimeout = 600 second(s), CreateChannelIfNotExists = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's BindListenerRequest in the specified amount of time (1000 milliseconds).
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket().Length.TestGreaterThan( 0 ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnError_WhenClientIsDisposedBeforeServerResponds()
        {
            var endSource = new SafeTaskCompletionSource<Task>();
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
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerClientTraceEventType.Ping )
                                    endSource.Complete( e.Source.Client.DisposeAsync().AsTask() );
                            } ) ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await endSource.Task.Unwrap();

            Assertion.All(
                    result.Value.TestNull(),
                    result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithListenerBoundResponseWithInvalidValues()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, channelId: 0, queueId: -1 );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.ListenerBoundResponse ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', QueueName = 'foo', PrefetchHint = 1, MaxRetries = 0, MaxRedeliveries = 0, MinAckTimeout = 600 second(s), CreateChannelIfNotExists = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 14)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid ListenerBoundResponse from the server. Encountered 2 error(s):
                                1. Expected channel ID to be greater than 0 but found 0.
                                2. Expected queue ID to be greater than 0 but found -1.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerBoundResponse, Length = 14)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithListenerBoundResponseWithInvalidPayload()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1, payload: 8 );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.ListenerBoundResponse ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', QueueName = 'foo', PrefetchHint = 1, MaxRetries = 0, MaxRedeliveries = 0, MinAckTimeout = 600 second(s), CreateChannelIfNotExists = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 13)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid ListenerBoundResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 9 but found 8.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerBoundResponse, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnError_WhenServerRespondsWithBindListenerFailureResponse()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendBindListenerFailureResponse( true, true, true );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.BindListenerRequest ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', QueueName = 'foo', PrefetchHint = 1, MaxRetries = 0, MaxRedeliveries = 0, MinAckTimeout = 600 second(s), CreateChannelIfNotExists = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerFailureResponse, Length = 6)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindListenerFailureResponse, Length = 6)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Server rejected an invalid BindListenerRequest sent by client 'test'. Encountered 3 error(s):
                                1. Channel 'foo' does not exist.
                                2. Client is already bound as a listener to channel 'foo'.
                                3. Binding client to channel 'foo' as a listener has been cancelled by the server.
                                """,
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerFailureResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithBindListenerFailureResponseWithInvalidPayload()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendBindListenerFailureResponse( true, true, true, payload: 0 );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.BindListenerFailureResponse ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', QueueName = 'foo', PrefetchHint = 1, MaxRetries = 0, MaxRedeliveries = 0, MinAckTimeout = 600 second(s), CreateChannelIfNotExists = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerFailureResponse, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid BindListenerFailureResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 1 but found 0.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerFailureResponse, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithInvalidEndpoint()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', QueueName = 'foo', PrefetchHint = 1, MaxRetries = 0, MaxRedeliveries = 0, MinAckTimeout = 600 second(s), CreateChannelIfNotExists = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 29)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                                1. Received unexpected client endpoint.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (<unrecognized-endpoint-0>, Length = 5)"
                        ] ) )
                .Go();
        }

        [Theory]
        [InlineData( true, true )]
        [InlineData( false, true )]
        [InlineData( true, false )]
        [InlineData( false, false )]
        public async Task UnbindAsync_ShouldUnbindListenerCorrectly(bool channelRemoved, bool queueRemoved)
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

            var channelId = 1;
            var channelName = "foo";
            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( channelName ) );
                    s.SendListenerBoundResponse( true, true, channelId, 2 );
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( channelRemoved, queueRemoved );
                } );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            await client.Listeners.BindAsync( channelName, (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( channelId );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestFalse(),
                    result.Value.ChannelRemoved.TestEquals( channelRemoved ),
                    result.Value.QueueRemoved.TestEquals( queueRemoved ),
                    result.Value.ToString()
                        .TestEquals(
                            channelRemoved
                                ? queueRemoved ? "Success (channel removed) (queue removed)" : "Success (channel removed)"
                                : queueRemoved
                                    ? "Success (queue removed)"
                                    : "Success" ),
                    listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                    client.Listeners.Count.TestEquals( 0 ),
                    client.Listeners.GetAll().TestEmpty(),
                    client.Listeners.TryGetByChannelName( channelName ).TestNull(),
                    client.Listeners.TryGetByChannelId( channelId ).TestNull(),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                                $"[UnbindingListener] Client = [1] 'test', TraceId = 2, Channel = [1] '{channelName}', Queue = [2] '{channelName}'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (ListenerUnboundResponse, Length = 6)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (ListenerUnboundResponse, Length = 6)",
                                $"[ListenerUnbound] Client = [1] 'test', TraceId = 2, Channel = [1] '{channelName}'{(channelRemoved ? " (removed)" : string.Empty)}, Queue = [2] '{channelName}'{(queueRemoved ? " (removed)" : string.Empty)}",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerUnboundResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldNotThrow_WhenListenerIsAlreadyLocallyDisposed()
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

            var channelName = "foo";
            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( channelName ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            await client.Listeners.BindAsync( channelName, (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );
            if ( listener is not null )
            {
                await listener.UnbindAsync();
                result = await listener.UnbindAsync();
            }

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestTrue(),
                    result.Value.ChannelRemoved.TestFalse(),
                    result.Value.ToString().TestEquals( "Not bound" ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var listener = new MessageBrokerListener(
                client,
                1,
                "foo",
                1,
                "foo",
                1,
                0,
                Duration.Zero,
                0,
                MessageBrokerListenerOptions.DefaultMinAckTimeout,
                (_, _) => ValueTask.CompletedTask );

            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await listener.UnbindAsync();
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerDoesNotRespondInTime()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            var listener = client.Listeners.TryGetByChannelId( 1 );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.UnbindListenerRequest ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                                "[UnbindingListener] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's UnbindListenerRequest in the specified amount of time (1000 milliseconds).
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket().Length.TestGreaterThan( 0 ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnError_WhenClientIsDisposedBeforeServerResponds()
        {
            var listenerBound = Atomic.Create( false );
            var endSource = new SafeTaskCompletionSource<Task>();
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
                            traceStart: e =>
                            {
                                var bound = listenerBound.Value;
                                if ( bound && e.Type == MessageBrokerClientTraceEventType.Ping )
                                    endSource.Complete( e.Source.Client.DisposeAsync().AsTask() );
                            } ) ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;
            var listener = client.Listeners.TryGetByChannelId( 1 );
            listenerBound.Value = true;

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await endSource.Task.Unwrap();

            result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithListenerUnboundResponseWithInvalidPayload()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true, payload: 0 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.ListenerUnboundResponse ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                                "[UnbindingListener] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (ListenerUnboundResponse, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid ListenerUnboundResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 1 but found 0.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerUnboundResponse, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnError_WhenServerRespondsWithUnbindListenerFailureResponse()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.SendUnbindListenerFailureResponse( true );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.UnbindListenerRequest ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                                "[UnbindingListener] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerFailureResponse, Length = 6)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerFailureResponse, Length = 6)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Server rejected an invalid UnbindListenerRequest sent by client 'test'. Encountered 1 error(s):
                                1. Client is not bound as a listener to channel [1] 'foo'.
                                """,
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerFailureResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task
            UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithUnbindListenerFailureResponseWithInvalidPayload()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.SendUnbindListenerFailureResponse( true, payload: 0 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.UnbindListenerFailureResponse ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                                "[UnbindingListener] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerFailureResponse, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid UnbindListenerFailureResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 1 but found 0.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerFailureResponse, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithInvalidEndpoint()
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                                "[UnbindingListener] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                                1. Received unexpected client endpoint.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (<unrecognized-endpoint-0>, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_ShouldInvokeCallback()
        {
            var endSource = new SafeTaskCompletionSource<(MessageBrokerListenerCallbackArgs Args, byte[] Message)>();
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

            var pushedAt = client.GetTimestamp();
            var message = new byte[] { 1, 2, 3, 4 };

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "sender2" );
                    s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 3, "stream3" );
                    s.SendMessageNotification( 1, 1, 2, 1, 3, message, pushedAt, true, 4, false, 5 );
                } );

            await client.Listeners.BindAsync(
                "foo",
                (a, _) =>
                {
                    endSource.Complete( (a, a.Data.ToArray()) );
                    return ValueTask.CompletedTask;
                },
                MessageBrokerListenerOptions.Default.SetRetryPolicy( 5 ).SetMaxRedeliveries( 5 ) );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            await serverTask;
            var (args, caughtMessage) = await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    args.Listener.TestRefEquals( listener ),
                    args.PushedAt.TestEquals( pushedAt ),
                    args.ReceivedAt.TestGreaterThanOrEqualTo( args.PushedAt ),
                    args.AckId.TestEquals( 1 ),
                    args.MessageId.TestEquals( 1UL ),
                    args.Sender.TestEquals( new MessageBrokerExternalObject( 2, "sender2" ) ),
                    args.Stream.TestEquals( new MessageBrokerExternalObject( 3, "stream3" ) ),
                    args.IsFirst.TestFalse(),
                    args.IsRetry.TestTrue(),
                    args.Retry.TestEquals( 4 ),
                    args.IsRedelivery.TestFalse(),
                    args.Redelivery.TestEquals( 5 ),
                    args.TraceId.TestEquals( 4UL ),
                    caughtMessage.TestSequence( message ),
                    args.ToString()
                        .TestEquals(
                            $"Listener = ([1] 'test' => [1] 'foo' listener (using [1] 'foo' queue) (Bound)), Stream = (Id = 3, Name = 'stream3'), AckId = 1, Id = 1, Retry = 4 (active), Redelivery = 5, Length = 4, PushedAt = {args.PushedAt}, ReceivedAt = {args.ReceivedAt}, Sender = (Id = 2, Name = 'sender2'), TraceId = 4" ),
                    logs.GetAll()
                        .Skip( 4 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 49)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, AckId = 1, StreamId = 3, MessageId = 1, Retry = 4 (active), Redelivery = 5, ChannelId = 1, SenderId = 2, Length = 4",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 49)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo', Queue = [1] 'foo', StreamId = 3, MessageId = 1, Retry = 4, Redelivery = 5",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 49)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_ShouldWaitWithCallbackInvocationUntilPreviousInvocationFinishes()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var continuationSource = new SafeTaskCompletionSource( completionCount: 2 );
            var invocationEndSourceIndex = 0;
            var invocationEndSources = new[]
            {
                new SafeTaskCompletionSource<(MessageBrokerListenerCallbackArgs Args, byte[] Message)>(),
                new SafeTaskCompletionSource<(MessageBrokerListenerCallbackArgs Args, byte[] Message)>()
            };

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
                                },
                                readPacket: e =>
                                {
                                    if ( e.Type == MessageBrokerClientReadPacketEventType.Accepted
                                        && e.Packet.Endpoint == MessageBrokerClientEndpoint.MessageNotification )
                                        continuationSource.Complete();
                                } ) ) ) );

            var message1 = new byte[] { 1, 2, 3, 4 };
            var message2 = new byte[] { 5, 6, 7, 8, 9 };

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "sender2" );
                    s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "stream1" );
                    s.SendMessageNotification( 1, 1, 2, 1, 1, message1 );
                    s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 3, "sender3" );
                    s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 2, "stream2" );
                    s.SendMessageNotification( 2, 2, 3, 1, 2, message2 );
                } );

            await client.Listeners.BindAsync(
                "foo",
                async (a, ct) =>
                {
                    var invocationEndSource = invocationEndSources[invocationEndSourceIndex++];
                    if ( invocationEndSourceIndex == 1 )
                    {
                        await continuationSource.Task;
                        await Task.Delay( 15, ct );
                        logs.Add( 4, "1st invocation" );
                        logs.Add( 7, "1st invocation" );
                    }
                    else
                        logs.Add( 7, "2nd invocation" );

                    invocationEndSource.Complete( (a, a.Data.ToArray()) );
                } );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            await serverTask;
            var (args1, caughtMessage1) = await invocationEndSources[0].Task;
            var (args2, caughtMessage2) = await invocationEndSources[1].Task;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    args1.Listener.TestRefEquals( listener ),
                    args1.AckId.TestEquals( 1 ),
                    args1.MessageId.TestEquals( 1UL ),
                    args1.IsFirst.TestTrue(),
                    args1.Sender.TestEquals( new MessageBrokerExternalObject( 2, "sender2" ) ),
                    args1.Stream.TestEquals( new MessageBrokerExternalObject( 1, "stream1" ) ),
                    args1.TraceId.TestEquals( 4UL ),
                    caughtMessage1.TestSequence( message1 ),
                    args2.Listener.TestRefEquals( listener ),
                    args2.AckId.TestEquals( 2 ),
                    args2.MessageId.TestEquals( 2UL ),
                    args2.IsFirst.TestTrue(),
                    args2.Sender.TestEquals( new MessageBrokerExternalObject( 3, "sender3" ) ),
                    args2.Stream.TestEquals( new MessageBrokerExternalObject( 2, "stream2" ) ),
                    args2.TraceId.TestEquals( 7UL ),
                    caughtMessage2.TestSequence( message2 ),
                    logs.GetAll()
                        .Where( t => t.Logs.Any( e => e.Contains( "[Trace:MessageNotification]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 49)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 4",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 49)",
                                "1st invocation",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo', Queue = [1] 'foo', StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 7 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 7, Packet = (MessageNotification, Length = 50)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 7, AckId = 2, StreamId = 2, MessageId = 2, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 3, Length = 5",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 7, Packet = (MessageNotification, Length = 50)",
                                "1st invocation",
                                "2nd invocation",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 7, Channel = [1] 'foo', Queue = [1] 'foo', StreamId = 2, MessageId = 2, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 7 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 49)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 50)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WhoseCallbackThrows_ShouldBeLoggedAndNegativeAckShouldBeSentAutomatically()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var exception = new Exception( "foo" );
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
                                    if ( e.Type is MessageBrokerClientTraceEventType.MessageNotification
                                        or MessageBrokerClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 1, 2, 1, 1, [ ] );
                    s.ReadMessageNotificationNegativeAck();
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => throw exception );
            await serverTask;
            var listener = client.Listeners.TryGetByChannelId( 1 );
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    listener.TestNotNull( l => l.State.TestEquals( MessageBrokerListenerState.Bound ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                (e, _) => e.TestEquals( "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)" ),
                                (e, _) => e.TestEquals(
                                    "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 45)" ),
                                (e, _) => e.TestEquals(
                                    "[ProcessingMessage] Client = [1] 'test', TraceId = 2, AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 0" ),
                                (e, _) => e.TestEquals(
                                    "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 45)" ),
                                (e, _) => e.TestStartsWith(
                                    """
                                    [Error] Client = [1] 'test', TraceId = 2
                                    System.Exception: foo
                                    """ ),
                                (e, _) => e.TestEquals(
                                    "[MessageProcessed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0" ),
                                (e, _) => e.TestEquals( "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)" )
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 3 (start)",
                                "[AcknowledgingMessage] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, MessageTraceId = 2, NACK = (SkipRetry = False, IsAutomatic = True)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (MessageNotificationNack, Length = 38)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (MessageNotificationNack, Length = 38)",
                                "[MessageAcknowledged] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 3 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 45)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WhoseCallbackThrows_ShouldOnlyBeLogged_WhenAcksAreDisabled()
        {
            var endSource = new SafeTaskCompletionSource();
            var exception = new Exception( "foo" );
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
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 0, 1, 2, 1, 1, [ ] );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => throw exception, MessageBrokerListenerOptions.Default.EnableAcks( false ) );
            await serverTask;
            var listener = client.Listeners.TryGetByChannelId( 1 );
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    listener.TestNotNull( l => l.State.TestEquals( MessageBrokerListenerState.Bound ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                (e, _) => e.TestEquals( "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)" ),
                                (e, _) => e.TestEquals(
                                    "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 45)" ),
                                (e, _) => e.TestEquals(
                                    "[ProcessingMessage] Client = [1] 'test', TraceId = 2, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 0" ),
                                (e, _) => e.TestEquals(
                                    "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 45)" ),
                                (e, _) => e.TestStartsWith(
                                    """
                                    [Error] Client = [1] 'test', TraceId = 2
                                    System.Exception: foo
                                    """ ),
                                (e, _) => e.TestEquals(
                                    "[MessageProcessed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0" ),
                                (e, _) => e.TestEquals( "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)" )
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 45)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WithInvalidPayload_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
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
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 1, 2, 1, 1, [ 1 ], payload: 39 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 44)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid MessageNotification from the server. Encountered 1 error(s):
                                1. Expected header payload to be at least 40 but found 39.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 44)"
                        ] ) )
                .Go();
        }

        [Theory]
        [InlineData(
            true,
            0,
            true,
            0,
            """
            Encountered 7 error(s):
            1. Expected ACK ID to not be negative but found -1.
            2. Expected stream ID to be greater than 0 but found -3.
            3. Message notification cannot be marked as both a retry and a redelivery.
            4. Expected retry to be greater than 0 but found 0.
            5. Expected redelivery to be greater than 0 but found 0.
            6. Expected channel ID to be greater than 0 but found 0.
            7. Expected sender ID to be greater than 0 but found -2.
            """ )]
        [InlineData(
            true,
            0,
            false,
            0,
            """
            Encountered 5 error(s):
            1. Expected ACK ID to not be negative but found -1.
            2. Expected stream ID to be greater than 0 but found -3.
            3. Expected retry to be greater than 0 but found 0.
            4. Expected channel ID to be greater than 0 but found 0.
            5. Expected sender ID to be greater than 0 but found -2.
            """ )]
        [InlineData(
            false,
            0,
            true,
            0,
            """
            Encountered 5 error(s):
            1. Expected ACK ID to not be negative but found -1.
            2. Expected stream ID to be greater than 0 but found -3.
            3. Expected redelivery to be greater than 0 but found 0.
            4. Expected channel ID to be greater than 0 but found 0.
            5. Expected sender ID to be greater than 0 but found -2.
            """ )]
        [InlineData(
            false,
            1,
            false,
            1,
            """
            Encountered 5 error(s):
            1. Expected ACK ID to not be negative but found -1.
            2. Expected stream ID to be greater than 0 but found -3.
            3. Message notification with retry 1 and redelivery 1 is not marked as either a retry or a redelivery.
            4. Expected channel ID to be greater than 0 but found 0.
            5. Expected sender ID to be greater than 0 but found -2.
            """ )]
        public async Task MessageNotification_WithInvalidProperties_ShouldDisposeClient(
            bool isRetry,
            int retryAttempt,
            bool isRedelivery,
            int redeliveryAttempt,
            string expectedError)
        {
            var endSource = new SafeTaskCompletionSource();
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
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification(
                        -1,
                        0,
                        -2,
                        0,
                        -3,
                        [ 1 ],
                        isRetry: isRetry,
                        retryAttempt: retryAttempt,
                        isRedelivery: isRedelivery,
                        redeliveryAttempt: redeliveryAttempt );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 46)",
                                $"""
                                 [Error] Client = [1] 'test', TraceId = 2
                                 LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid MessageNotification from the server. {expectedError}
                                 """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 46)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WithInvalidListenerProperties_ShouldBeIgnored()
        {
            var endSource = new SafeTaskCompletionSource();
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
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification(
                        0,
                        0,
                        2,
                        1,
                        1,
                        [ 1 ],
                        isRetry: true,
                        retryAttempt: 2,
                        isRedelivery: false,
                        redeliveryAttempt: 3 );
                } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.SetRetryPolicy( 1 ).SetMaxRedeliveries( 2 ) );

            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 46)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 2, StreamId = 1, MessageId = 0, Retry = 2 (active), Redelivery = 3, ChannelId = 1, SenderId = 2, Length = 1",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientMessageException: Client [1] 'test' received an invalid message notification through channel [1] 'foo'. Encountered 3 error(s):
                                1. Expected ACK ID to be greater than 0 because listener has ACKs enabled.
                                2. Retry 2 exceeds listener's 1 max retries.
                                3. Redelivery 3 exceeds listener's 2 max redeliveries.
                                """,
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 46)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WithInvalidListenerProperties_ShouldBeIgnored_WhenAcksAreDisabled()
        {
            var endSource = new SafeTaskCompletionSource();
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
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 0, 2, 1, 1, [ 1 ] );
                } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.EnableAcks( false ) );

            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 46)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 2, AckId = 1, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 1",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientMessageException: Client [1] 'test' received an invalid message notification through channel [1] 'foo'. Encountered 1 error(s):
                                1. Expected ACK ID to be equal to 0 because listener has ACKs disabled.
                                """,
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 46)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WithNonExistingListener_ShouldBeIgnored()
        {
            var endSource = new SafeTaskCompletionSource();
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
            var serverTask = server.GetTask( s => { s.SendMessageNotification( 1, 1, 1, 1, 1, [ 1, 2, 3, 4 ] ); } );

            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (MessageNotification, Length = 49)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 1, AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 1, Length = 4",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientMessageException: Listener for channel with ID 1 does not exist.
                                """,
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 49)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Disposal_ShouldDiscardPendingMessages()
        {
            Exception? unbindException = null;
            var endSource = new SafeTaskCompletionSource( completionCount: 3 );
            var messageReceivedContinuation = new SafeTaskCompletionSource( completionCount: 3 );
            var callbackContinuation = new SafeTaskCompletionSource( completionCount: 3 );
            var unboundContinuation = new SafeTaskCompletionSource();

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
                                    if ( e.Type == MessageBrokerClientTraceEventType.UnbindListener )
                                    {
                                        unboundContinuation.Complete();
                                        endSource.Complete();
                                    }
                                    else if ( e.Type == MessageBrokerClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                },
                                awaitPacket: e =>
                                {
                                    if ( e.Packet?.Endpoint == MessageBrokerClientEndpoint.MessageNotification )
                                        messageReceivedContinuation.Complete();
                                },
                                readPacket: e =>
                                {
                                    if ( e.Packet.Endpoint != MessageBrokerClientEndpoint.MessageNotification )
                                        return;

                                    if ( e.Type == MessageBrokerClientReadPacketEventType.Received )
                                        messageReceivedContinuation.Task.Wait();
                                    else if ( callbackContinuation.Complete() )
                                        unboundContinuation.Task.Wait();
                                },
                                error: e =>
                                {
                                    if ( e.Source.TraceId == 5 )
                                        unbindException = e.Exception;
                                } ) ) ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 1, 2, 1, 1, [ 1, 2 ] );
                    s.SendMessageNotification( 2, 2, 2, 1, 1, [ 3, 4, 5 ] );
                    s.SendMessageNotification( 3, 3, 2, 1, 1, [ 6, 7, 8, 9 ] );
                } );

            var bindResult = await client.Listeners.BindAsync(
                "foo",
                async (a, ct) =>
                {
                    await callbackContinuation.Task;
                    await Task.Delay( 100, ct );
                    _ = a.Listener.UnbindAsync().AsTask();
                } );

            var listener = bindResult.Value?.Listener;

            await serverTask;
            await server.GetTask(
                s =>
                {
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            await endSource.Task;

            Assertion.All(
                    unbindException.TestType()
                        .Exact<MessageBrokerClientMessageException>(
                            exc => Assertion.All( exc.Client.TestRefEquals( client ), exc.Listener.TestRefEquals( listener ) ) ),
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 47)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 2, AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 2",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 47)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo', StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 3 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (MessageNotification, Length = 48)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 3, AckId = 2, StreamId = 1, MessageId = 2, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 3",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (MessageNotification, Length = 48)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 49)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, AckId = 3, StreamId = 1, MessageId = 3, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 4",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 49)",
                                """
                                [Error] Client = [1] 'test', TraceId = 4
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientMessageException: Listener for channel with ID 1 does not exist.
                                """,
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => Assertion.All(
                                t.Logs.TestContainsSequence(
                                [
                                    "[Trace:UnbindListener] Client = [1] 'test', TraceId = 5 (start)",
                                    "[UnbindingListener] Client = [1] 'test', TraceId = 5, Channel = [1] 'foo', Queue = [1] 'foo'",
                                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (UnbindListenerRequest, Length = 9)",
                                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (UnbindListenerRequest, Length = 9)",
                                    "[ReadPacket:Received] Client = [1] 'test', TraceId = 5, Packet = (ListenerUnboundResponse, Length = 6)",
                                    "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 5, Packet = (ListenerUnboundResponse, Length = 6)",
                                    "[ListenerUnbound] Client = [1] 'test', TraceId = 5, Channel = [1] 'foo' (removed), Queue = [1] 'foo' (removed)",
                                    "[Trace:UnbindListener] Client = [1] 'test', TraceId = 5 (end)"
                                ] ),
                                t.Logs.TestContainsSequence(
                                [
                                    """
                                    [Error] Client = [1] 'test', TraceId = 5
                                    LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientMessageException: 1 locally stored message notification(s) by [1] 'foo' channel listener have been discarded due to listener disposal.
                                    """
                                ] ) )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 47)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 48)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 49)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerUnboundResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ClientDisposal_ShouldDiscardPendingMessagesWhichAreNotYetPropagatedToListeners()
        {
            Exception? disposeException = null;
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var messageReceivedContinuation = new SafeTaskCompletionSource( completionCount: 2 );
            var disposeContinuation = new SafeTaskCompletionSource();

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
                                    if ( e.Type is MessageBrokerClientTraceEventType.MessageNotification
                                        or MessageBrokerClientTraceEventType.Dispose )
                                        endSource.Complete();
                                },
                                disposing: _ => disposeContinuation.Complete(),
                                awaitPacket: e =>
                                {
                                    if ( e.Packet?.Endpoint == MessageBrokerClientEndpoint.MessageNotification )
                                        messageReceivedContinuation.Complete();
                                },
                                readPacket: e =>
                                {
                                    if ( e.Packet.Endpoint != MessageBrokerClientEndpoint.MessageNotification )
                                        return;

                                    if ( e.Type == MessageBrokerClientReadPacketEventType.Received )
                                    {
                                        messageReceivedContinuation.Task.Wait();
                                        Thread.Sleep( 100 );
                                        _ = e.Source.Client.DisposeAsync().AsTask();
                                        disposeContinuation.Task.Wait();
                                    }
                                },
                                error: e =>
                                {
                                    if ( e.Source.TraceId == 2 )
                                        disposeException = e.Exception;
                                } ) ) ) );

            await server.EstablishHandshake( client );
            await server.GetTask(
                s =>
                {
                    s.SendMessageNotification( 1, 1, 2, 1, 1, [ 1, 2 ] );
                    s.SendMessageNotification( 1, 2, 2, 1, 1, [ 3, 4, 5 ] );
                } );

            await endSource.Task;

            Assertion.All(
                    disposeException.TestType()
                        .Exact<MessageBrokerClientMessageException>(
                            exc => Assertion.All( exc.Client.TestRefEquals( client ), exc.Listener.TestNull() ) ),
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (MessageNotification, Length = 47)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 1, AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 2",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientMessageException: Listener for channel with ID 1 does not exist.
                                """,
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 1 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (start)",
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientMessageException: 1 locally stored message notification(s) have been discarded due to client disposal.
                                """,
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 47)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 48)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Disposal_ShouldCancelCallbackCancellationToken()
        {
            var unbindContinuation = new SafeTaskCompletionSource<MessageBrokerListener>();
            var cancellationSource = new SafeTaskCompletionSource();
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 1, 2, 1, 1, [ 1, 2, 3, 4 ] );
                } );

            await client.Listeners.BindAsync(
                "foo",
                async (a, ct) =>
                {
                    ct.UnsafeRegister( _ => cancellationSource.Complete(), null );
                    unbindContinuation.Complete( a.Listener );
                    await cancellationSource.Task;
                    ct.ThrowIfCancellationRequested();
                } );

            await serverTask;
            var listener = await unbindContinuation.Task;
            serverTask = server.GetTask(
                s =>
                {
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            await listener.UnbindAsync();
            await serverTask;
            await cancellationSource.Task;

            Assertion.All( client.State.TestEquals( MessageBrokerClientState.Running ) ).Go();
        }

        [Fact]
        public async Task Disposal_ShouldLogCallbackCancellationTokenRegistrationException()
        {
            var exception = new Exception( "foo" );
            var unbindContinuation = new SafeTaskCompletionSource<MessageBrokerListener>();
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
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 1, 2, 1, 1, [ 1, 2, 3, 4 ] );
                } );

            await client.Listeners.BindAsync(
                "foo",
                (a, ct) =>
                {
                    ct.UnsafeRegister( _ => throw exception, null );
                    unbindContinuation.Complete( a.Listener );
                    return ValueTask.CompletedTask;
                } );

            await serverTask;
            var listener = await unbindContinuation.Task;
            serverTask = server.GetTask(
                s =>
                {
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            await listener.UnbindAsync();
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    listener.TestNotNull( l => l.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                    logs.GetAll()
                        .Skip( 3 )
                        .TestSequence(
                        [
                            (t, _) => Assertion.All(
                                t.Logs.TestContainsSequence(
                                [
                                    "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (start)",
                                    "[UnbindingListener] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo'",
                                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (UnbindListenerRequest, Length = 9)",
                                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (UnbindListenerRequest, Length = 9)",
                                    "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (ListenerUnboundResponse, Length = 6)",
                                    "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (ListenerUnboundResponse, Length = 6)",
                                    "[ListenerUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo' (removed), Queue = [1] 'foo' (removed)",
                                    "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (end)"
                                ] ),
                                t.Logs.TestContainsSequence(
                                [
                                    (e, _) => Assertion.All(
                                        e.TestStartsWith(
                                            """
                                            [Error] Client = [1] 'test', TraceId = 3
                                            System.AggregateException:
                                            """ ),
                                        e.TestContains( "System.Exception: foo" ) )
                                ] ) )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerUnboundResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Disposal_ShouldLogInvocationTimeoutAndStopWaiting()
        {
            var unbindContinuation = new SafeTaskCompletionSource<MessageBrokerListener>();
            var invocationContinuation = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetListenerDisposalTimeout( Duration.FromMilliseconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerClientTraceEventType.UnbindListener )
                                        invocationContinuation.Complete();
                                } ) ) ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( GetBindListenerRequest( "foo" ) );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 1, 2, 1, 1, [ 1, 2, 3, 4 ] );
                } );

            await client.Listeners.BindAsync(
                "foo",
                async (a, _) =>
                {
                    unbindContinuation.Complete( a.Listener );
                    await invocationContinuation.Task;
                } );

            await serverTask;
            var listener = await unbindContinuation.Task;
            serverTask = server.GetTask(
                s =>
                {
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            await listener.UnbindAsync();
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    listener.TestNotNull( l => l.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                    logs.GetAll()
                        .Skip( 3 )
                        .TestSequence(
                        [
                            (t, _) => Assertion.All(
                                t.Logs.TestContainsSequence(
                                [
                                    "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (start)",
                                    "[UnbindingListener] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo'",
                                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (UnbindListenerRequest, Length = 9)",
                                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (UnbindListenerRequest, Length = 9)",
                                    "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (ListenerUnboundResponse, Length = 6)",
                                    "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (ListenerUnboundResponse, Length = 6)",
                                    "[ListenerUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo' (removed), Queue = [1] 'foo' (removed)",
                                    "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (end)"
                                ] ),
                                t.Logs.TestContainsSequence(
                                [
                                    (e, _) => e.TestStartsWith(
                                        """
                                        [Error] Client = [1] 'test', TraceId = 3
                                        System.TimeoutException:
                                        """ )
                                ] ) )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerUnboundResponse, Length = 6)"
                        ] ) )
                .Go();
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
            bool createChannelIfNotExists = true)
        {
            return new Protocol.BindListenerRequest(
                channelName,
                queueName,
                prefetchHint,
                maxRetries,
                retryDelay,
                maxRedeliveries,
                minAckTimeout ?? MessageBrokerListenerOptions.DefaultMinAckTimeout,
                createChannelIfNotExists );
        }
    }
}
