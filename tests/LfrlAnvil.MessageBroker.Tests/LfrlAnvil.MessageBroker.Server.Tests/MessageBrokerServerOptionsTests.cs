using System.IO;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
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
                sut.EventHandler.TestNull(),
                sut.ClientEventHandlerFactory.TestNull(),
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
                result.EventHandler.TestEquals( sut.EventHandler ),
                result.ClientEventHandlerFactory.TestEquals( sut.ClientEventHandlerFactory ),
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
                result.EventHandler.TestEquals( sut.EventHandler ),
                result.ClientEventHandlerFactory.TestEquals( sut.ClientEventHandlerFactory ),
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
                result.EventHandler.TestEquals( sut.EventHandler ),
                result.ClientEventHandlerFactory.TestEquals( sut.ClientEventHandlerFactory ),
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
                result.EventHandler.TestEquals( sut.EventHandler ),
                result.ClientEventHandlerFactory.TestEquals( sut.ClientEventHandlerFactory ),
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
                result.EventHandler.TestEquals( sut.EventHandler ),
                result.ClientEventHandlerFactory.TestEquals( sut.ClientEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetEventHandler_ShouldChangeValue()
    {
        MessageBrokerServerEventHandler value = _ => { };
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetEventHandler( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.EventHandler.TestEquals( value ),
                result.ClientEventHandlerFactory.TestEquals( sut.ClientEventHandlerFactory ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetClientEventHandlerFactory_ShouldChangeValue()
    {
        Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientEventHandler?> value = _ => _ => { };
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetClientEventHandlerFactory( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.HandshakeTimeout.TestEquals( sut.HandshakeTimeout ),
                result.AcceptableMessageTimeout.TestEquals( sut.AcceptableMessageTimeout ),
                result.AcceptablePingInterval.TestEquals( sut.AcceptablePingInterval ),
                result.EventHandler.TestEquals( sut.EventHandler ),
                result.ClientEventHandlerFactory.TestEquals( value ),
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
                result.EventHandler.TestEquals( sut.EventHandler ),
                result.ClientEventHandlerFactory.TestEquals( sut.ClientEventHandlerFactory ),
                result.StreamDecorator.TestRefEquals( value ) )
            .Go();
    }
}
