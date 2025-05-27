using System.IO;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerServerOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = MessageBrokerServerOptions.Default;
        Assertion.All(
                sut.Tcp.TestEquals( MessageBrokerTcpServerOptions.Default ),
                sut.MinMemoryPoolSegmentLength.TestNull(),
                sut.HandshakeTimeout.TestNull(),
                sut.AcceptableMessageTimeout.TestNull(),
                sut.AcceptablePingInterval.TestNull(),
                sut.TimestampsFactory.TestNull(),
                sut.DelaySourceFactory.TestNull(),
                sut.Logger.TestNull(),
                sut.ClientLoggerFactory.TestNull(),
                sut.ChannelLoggerFactory.TestNull(),
                sut.StreamEventHandlerFactory.TestNull(),
                sut.QueueEventHandlerFactory.TestNull(),
                sut.StreamDecorator.TestNull() )
            .Go();
    }

    [Fact]
    public void SetTcpOptions_ShouldChangeValue()
    {
        var value = MessageBrokerTcpServerOptions.Default.SetNoDelay( true ).SetSocketBufferSize( MemorySize.FromKilobytes( 16 ) );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetTcpOptions( value );

        Assertion.All(
                result.Tcp.TestEquals( value ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetMinMemoryPoolSegmentLength_ShouldChangeValue()
    {
        var value = MemorySize.FromKilobytes( 32 );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetMinMemoryPoolSegmentLength( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( value ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetHandshakeTimeout_ShouldChangeValue()
    {
        var value = Duration.FromSeconds( 20 );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetHandshakeTimeout( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( value ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetAcceptableMessageTimeout_ShouldChangeValue()
    {
        var value = Bounds.Create( Duration.FromSeconds( 5 ), Duration.FromSeconds( 30 ) );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetAcceptableMessageTimeout( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( value ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetDesiredPingInterval_ShouldChangeValue()
    {
        var value = Bounds.Create( Duration.FromSeconds( 5 ), Duration.FromSeconds( 30 ) );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetAcceptablePingInterval( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( value ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetTimestampsFactory_ShouldChangeValue()
    {
        Func<MessageBrokerRemoteClient, ITimestampProvider> value = _ => new TimestampProvider();
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetTimestampsFactory( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( value ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetDelaySourceFactory_ShouldChangeValue()
    {
        Func<MessageBrokerRemoteClient, ValueTaskDelaySource> value = _ => ValueTaskDelaySource.Start();
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetDelaySourceFactory( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( value ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetLogger_ShouldChangeValue()
    {
        var value = MessageBrokerServerLogger.Create( traceStart: _ => { } );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetLogger( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( value ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetClientLoggerFactory_ShouldChangeValue()
    {
        Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientLogger?> value = _ =>
            MessageBrokerRemoteClientLogger.Create( traceStart: _ => { } );

        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetClientLoggerFactory( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( value ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetChannelLoggerFactory_ShouldChangeValue()
    {
        Func<MessageBrokerChannel, MessageBrokerChannelLogger?> value = _ => MessageBrokerChannelLogger.Create( traceStart: _ => { } );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetChannelLoggerFactory( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( value ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetStreamEventHandlerFactory_ShouldChangeValue()
    {
        Func<MessageBrokerStream, MessageBrokerStreamEventHandler?> value = _ => _ => { };
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetStreamEventHandlerFactory( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( value ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetQueueEventHandlerFactory_ShouldChangeValue()
    {
        Func<MessageBrokerQueue, MessageBrokerQueueEventHandler?> value = _ => _ => { };
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetQueueEventHandlerFactory( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( result.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( value ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetStreamDecorator_ShouldChangeValue()
    {
        MessageBrokerRemoteClientStreamDecorator value = (_, s) => ValueTask.FromResult<Stream>( s );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetStreamDecorator( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.TimestampsFactory.TestEquals( sut.TimestampsFactory ),
                result.DelaySourceFactory.TestEquals( sut.DelaySourceFactory ),
                result.Logger.TestEquals( sut.Logger ),
                result.ClientLoggerFactory.TestEquals( sut.ClientLoggerFactory ),
                result.ChannelLoggerFactory.TestEquals( sut.ChannelLoggerFactory ),
                result.StreamEventHandlerFactory.TestEquals( sut.StreamEventHandlerFactory ),
                result.QueueEventHandlerFactory.TestEquals( sut.QueueEventHandlerFactory ),
                result.StreamDecorator.TestRefEquals( value ) )
            .Go();
    }
}
