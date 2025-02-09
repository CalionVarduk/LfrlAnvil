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
        using ( new AssertionScope() )
        {
            sut.Tcp.Should().Be( MessageBrokerTcpServerOptions.Default );
            sut.MinMemoryPoolSegmentLength.Should().BeNull();
            sut.HandshakeTimeout.Should().BeNull();
            sut.AcceptableMessageTimeout.Should().BeNull();
            sut.AcceptablePingInterval.Should().BeNull();
            sut.EventHandler.Should().BeNull();
            sut.ClientEventHandlerFactory.Should().BeNull();
            sut.StreamDecorator.Should().BeNull();
        }
    }

    [Fact]
    public void SetTcpOptions_ShouldChangeValue()
    {
        var value = MessageBrokerTcpServerOptions.Default.SetNoDelay( true ).SetSocketBufferSize( MemorySize.FromKilobytes( 16 ) );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetTcpOptions( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( value );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.HandshakeTimeout.Should().Be( sut.HandshakeTimeout );
            result.AcceptableMessageTimeout.Should().Be( sut.AcceptableMessageTimeout );
            result.AcceptablePingInterval.Should().Be( sut.AcceptablePingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.ClientEventHandlerFactory.Should().Be( sut.ClientEventHandlerFactory );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetMinMemoryPoolSegmentLength_ShouldChangeValue()
    {
        var value = MemorySize.FromKilobytes( 32 );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetMinMemoryPoolSegmentLength( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( value );
            result.HandshakeTimeout.Should().Be( sut.HandshakeTimeout );
            result.AcceptableMessageTimeout.Should().Be( sut.AcceptableMessageTimeout );
            result.AcceptablePingInterval.Should().Be( sut.AcceptablePingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.ClientEventHandlerFactory.Should().Be( sut.ClientEventHandlerFactory );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetHandshakeTimeout_ShouldChangeValue()
    {
        var value = Duration.FromSeconds( 20 );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetHandshakeTimeout( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.HandshakeTimeout.Should().Be( value );
            result.AcceptableMessageTimeout.Should().Be( sut.AcceptableMessageTimeout );
            result.AcceptablePingInterval.Should().Be( sut.AcceptablePingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.ClientEventHandlerFactory.Should().Be( sut.ClientEventHandlerFactory );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetAcceptableMessageTimeout_ShouldChangeValue()
    {
        var value = Bounds.Create( Duration.FromSeconds( 5 ), Duration.FromSeconds( 30 ) );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetAcceptableMessageTimeout( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.HandshakeTimeout.Should().Be( sut.HandshakeTimeout );
            result.AcceptableMessageTimeout.Should().Be( value );
            result.AcceptablePingInterval.Should().Be( sut.AcceptablePingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.ClientEventHandlerFactory.Should().Be( sut.ClientEventHandlerFactory );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetDesiredPingInterval_ShouldChangeValue()
    {
        var value = Bounds.Create( Duration.FromSeconds( 5 ), Duration.FromSeconds( 30 ) );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetAcceptablePingInterval( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.HandshakeTimeout.Should().Be( sut.HandshakeTimeout );
            result.AcceptableMessageTimeout.Should().Be( sut.AcceptableMessageTimeout );
            result.AcceptablePingInterval.Should().Be( value );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.ClientEventHandlerFactory.Should().Be( sut.ClientEventHandlerFactory );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetEventHandler_ShouldChangeValue()
    {
        MessageBrokerServerEventHandler value = _ => { };
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetEventHandler( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.HandshakeTimeout.Should().Be( sut.HandshakeTimeout );
            result.AcceptableMessageTimeout.Should().Be( sut.AcceptableMessageTimeout );
            result.AcceptablePingInterval.Should().Be( sut.AcceptablePingInterval );
            result.EventHandler.Should().Be( value );
            result.ClientEventHandlerFactory.Should().Be( sut.ClientEventHandlerFactory );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetClientEventHandlerFactory_ShouldChangeValue()
    {
        Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientEventHandler?> value = _ => _ => { };
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetClientEventHandlerFactory( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.HandshakeTimeout.Should().Be( sut.HandshakeTimeout );
            result.AcceptableMessageTimeout.Should().Be( sut.AcceptableMessageTimeout );
            result.AcceptablePingInterval.Should().Be( sut.AcceptablePingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.ClientEventHandlerFactory.Should().Be( value );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetStreamDecorator_ShouldChangeValue()
    {
        MessageBrokerRemoteClientStreamDecorator value = (_, s) => ValueTask.FromResult<Stream>( s );
        var sut = MessageBrokerServerOptions.Default;

        var result = sut.SetStreamDecorator( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.HandshakeTimeout.Should().Be( sut.HandshakeTimeout );
            result.AcceptableMessageTimeout.Should().Be( sut.AcceptableMessageTimeout );
            result.AcceptablePingInterval.Should().Be( sut.AcceptablePingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.ClientEventHandlerFactory.Should().Be( sut.ClientEventHandlerFactory );
            result.StreamDecorator.Should().BeSameAs( value );
        }
    }
}
