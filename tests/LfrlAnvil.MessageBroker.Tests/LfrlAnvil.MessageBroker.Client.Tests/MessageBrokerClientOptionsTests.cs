using System.IO;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerClientOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = MessageBrokerClientOptions.Default;
        using ( new AssertionScope() )
        {
            sut.Tcp.Should().Be( MessageBrokerTcpClientOptions.Default );
            sut.MinMemoryPoolSegmentLength.Should().BeNull();
            sut.ConnectionTimeout.Should().BeNull();
            sut.DesiredMessageTimeout.Should().BeNull();
            sut.DesiredPingInterval.Should().BeNull();
            sut.EventHandler.Should().BeNull();
            sut.StreamDecorator.Should().BeNull();
        }
    }

    [Fact]
    public void SetTcpOptions_ShouldChangeValue()
    {
        var value = MessageBrokerTcpClientOptions.Default.SetNoDelay( true ).SetSocketBufferSize( MemorySize.FromKilobytes( 16 ) );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetTcpOptions( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( value );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.ConnectionTimeout.Should().Be( sut.ConnectionTimeout );
            result.DesiredMessageTimeout.Should().Be( sut.DesiredMessageTimeout );
            result.DesiredPingInterval.Should().Be( sut.DesiredPingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetMinMemoryPoolSegmentLength_ShouldChangeValue()
    {
        var value = MemorySize.FromKilobytes( 32 );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetMinMemoryPoolSegmentLength( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( value );
            result.ConnectionTimeout.Should().Be( sut.ConnectionTimeout );
            result.DesiredMessageTimeout.Should().Be( sut.DesiredMessageTimeout );
            result.DesiredPingInterval.Should().Be( sut.DesiredPingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetConnectionTimeout_ShouldChangeValue()
    {
        var value = Duration.FromSeconds( 20 );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetConnectionTimeout( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.ConnectionTimeout.Should().Be( value );
            result.DesiredMessageTimeout.Should().Be( sut.DesiredMessageTimeout );
            result.DesiredPingInterval.Should().Be( sut.DesiredPingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetDesiredMessageTimeout_ShouldChangeValue()
    {
        var value = Duration.FromSeconds( 30 );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetDesiredMessageTimeout( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.ConnectionTimeout.Should().Be( sut.ConnectionTimeout );
            result.DesiredMessageTimeout.Should().Be( value );
            result.DesiredPingInterval.Should().Be( sut.DesiredPingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetDesiredPingInterval_ShouldChangeValue()
    {
        var value = Duration.FromSeconds( 30 );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetDesiredPingInterval( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.ConnectionTimeout.Should().Be( sut.ConnectionTimeout );
            result.DesiredMessageTimeout.Should().Be( sut.DesiredMessageTimeout );
            result.DesiredPingInterval.Should().Be( value );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetEventHandler_ShouldChangeValue()
    {
        MessageBrokerClientEventHandler value = _ => { };
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetEventHandler( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.ConnectionTimeout.Should().Be( sut.ConnectionTimeout );
            result.DesiredMessageTimeout.Should().Be( sut.DesiredMessageTimeout );
            result.DesiredPingInterval.Should().Be( sut.DesiredPingInterval );
            result.EventHandler.Should().BeSameAs( value );
            result.StreamDecorator.Should().Be( sut.StreamDecorator );
        }
    }

    [Fact]
    public void SetStreamDecorator_ShouldChangeValue()
    {
        MessageBrokerClientStreamDecorator value = (_, s, _) => ValueTask.FromResult<Stream>( s );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetStreamDecorator( value );

        using ( new AssertionScope() )
        {
            result.Tcp.Should().Be( sut.Tcp );
            result.MinMemoryPoolSegmentLength.Should().Be( sut.MinMemoryPoolSegmentLength );
            result.ConnectionTimeout.Should().Be( sut.ConnectionTimeout );
            result.DesiredMessageTimeout.Should().Be( sut.DesiredMessageTimeout );
            result.DesiredPingInterval.Should().Be( sut.DesiredPingInterval );
            result.EventHandler.Should().Be( sut.EventHandler );
            result.StreamDecorator.Should().BeSameAs( value );
        }
    }
}
