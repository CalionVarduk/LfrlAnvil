using System.IO;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerClientOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = MessageBrokerClientOptions.Default;
        Assertion.All(
                sut.Tcp.TestEquals( MessageBrokerTcpClientOptions.Default ),
                sut.MinMemoryPoolSegmentLength.TestNull(),
                sut.ConnectionTimeout.TestNull(),
                sut.DesiredMessageTimeout.TestNull(),
                sut.DesiredPingInterval.TestNull(),
                sut.ListenerDisposalTimeout.TestNull(),
                sut.Timestamps.TestNull(),
                sut.DelaySource.TestNull(),
                sut.Logger.TestNull(),
                sut.StreamDecorator.TestNull() )
            .Go();
    }

    [Fact]
    public void SetTcpOptions_ShouldChangeValue()
    {
        var value = MessageBrokerTcpClientOptions.Default.SetNoDelay( true ).SetSocketBufferSize( MemorySize.FromKilobytes( 16 ) );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetTcpOptions( value );

        Assertion.All(
                result.Tcp.TestEquals( value ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.ConnectionTimeout.TestEquals( sut.ConnectionTimeout ),
                result.DesiredMessageTimeout.TestEquals( sut.DesiredMessageTimeout ),
                result.DesiredPingInterval.TestEquals( sut.DesiredPingInterval ),
                result.ListenerDisposalTimeout.TestEquals( sut.ListenerDisposalTimeout ),
                result.Timestamps.TestEquals( sut.Timestamps ),
                result.DelaySource.TestEquals( sut.DelaySource ),
                result.Logger.TestEquals( sut.Logger ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetMinMemoryPoolSegmentLength_ShouldChangeValue()
    {
        var value = MemorySize.FromKilobytes( 32 );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetMinMemoryPoolSegmentLength( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( value ),
                result.ConnectionTimeout.TestEquals( sut.ConnectionTimeout ),
                result.DesiredMessageTimeout.TestEquals( sut.DesiredMessageTimeout ),
                result.DesiredPingInterval.TestEquals( sut.DesiredPingInterval ),
                result.ListenerDisposalTimeout.TestEquals( sut.ListenerDisposalTimeout ),
                result.Timestamps.TestEquals( sut.Timestamps ),
                result.DelaySource.TestEquals( sut.DelaySource ),
                result.Logger.TestEquals( sut.Logger ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetConnectionTimeout_ShouldChangeValue()
    {
        var value = Duration.FromSeconds( 20 );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetConnectionTimeout( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.ConnectionTimeout.TestEquals( value ),
                result.DesiredMessageTimeout.TestEquals( sut.DesiredMessageTimeout ),
                result.DesiredPingInterval.TestEquals( sut.DesiredPingInterval ),
                result.ListenerDisposalTimeout.TestEquals( sut.ListenerDisposalTimeout ),
                result.Timestamps.TestEquals( sut.Timestamps ),
                result.DelaySource.TestEquals( sut.DelaySource ),
                result.Logger.TestEquals( sut.Logger ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetDesiredMessageTimeout_ShouldChangeValue()
    {
        var value = Duration.FromSeconds( 30 );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetDesiredMessageTimeout( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.ConnectionTimeout.TestEquals( sut.ConnectionTimeout ),
                result.DesiredMessageTimeout.TestEquals( value ),
                result.DesiredPingInterval.TestEquals( sut.DesiredPingInterval ),
                result.ListenerDisposalTimeout.TestEquals( sut.ListenerDisposalTimeout ),
                result.Timestamps.TestEquals( sut.Timestamps ),
                result.DelaySource.TestEquals( sut.DelaySource ),
                result.Logger.TestEquals( sut.Logger ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetDesiredPingInterval_ShouldChangeValue()
    {
        var value = Duration.FromSeconds( 30 );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetDesiredPingInterval( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.ConnectionTimeout.TestEquals( sut.ConnectionTimeout ),
                result.DesiredMessageTimeout.TestEquals( sut.DesiredMessageTimeout ),
                result.DesiredPingInterval.TestEquals( value ),
                result.ListenerDisposalTimeout.TestEquals( sut.ListenerDisposalTimeout ),
                result.Timestamps.TestEquals( sut.Timestamps ),
                result.DelaySource.TestEquals( sut.DelaySource ),
                result.Logger.TestEquals( sut.Logger ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetListenerDisposalTimeout_ShouldChangeValue()
    {
        var value = Duration.FromSeconds( 30 );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetListenerDisposalTimeout( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.ConnectionTimeout.TestEquals( sut.ConnectionTimeout ),
                result.DesiredMessageTimeout.TestEquals( sut.DesiredMessageTimeout ),
                result.DesiredPingInterval.TestEquals( sut.DesiredPingInterval ),
                result.ListenerDisposalTimeout.TestEquals( value ),
                result.Timestamps.TestEquals( sut.Timestamps ),
                result.DelaySource.TestEquals( sut.DelaySource ),
                result.Logger.TestEquals( sut.Logger ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetTimestamps_ShouldChangeValue()
    {
        var value = new TimestampProvider();
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetTimestamps( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.ConnectionTimeout.TestEquals( sut.ConnectionTimeout ),
                result.DesiredMessageTimeout.TestEquals( sut.DesiredMessageTimeout ),
                result.DesiredPingInterval.TestEquals( sut.DesiredPingInterval ),
                result.ListenerDisposalTimeout.TestEquals( sut.ListenerDisposalTimeout ),
                result.Timestamps.TestEquals( value ),
                result.DelaySource.TestEquals( sut.DelaySource ),
                result.Logger.TestEquals( sut.Logger ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetDelaySource_ShouldChangeValue()
    {
        using var value = ValueTaskDelaySource.Start();
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetDelaySource( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.ConnectionTimeout.TestEquals( sut.ConnectionTimeout ),
                result.DesiredMessageTimeout.TestEquals( sut.DesiredMessageTimeout ),
                result.DesiredPingInterval.TestEquals( sut.DesiredPingInterval ),
                result.ListenerDisposalTimeout.TestEquals( sut.ListenerDisposalTimeout ),
                result.Timestamps.TestEquals( sut.Timestamps ),
                result.DelaySource.TestEquals( value ),
                result.Logger.TestEquals( sut.Logger ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetLogger_ShouldChangeValue()
    {
        var value = MessageBrokerClientLogger.Create( traceStart: _ => { } );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetLogger( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.ConnectionTimeout.TestEquals( sut.ConnectionTimeout ),
                result.DesiredMessageTimeout.TestEquals( sut.DesiredMessageTimeout ),
                result.DesiredPingInterval.TestEquals( sut.DesiredPingInterval ),
                result.ListenerDisposalTimeout.TestEquals( sut.ListenerDisposalTimeout ),
                result.Timestamps.TestEquals( sut.Timestamps ),
                result.DelaySource.TestEquals( sut.DelaySource ),
                result.Logger.TestEquals( value ),
                result.StreamDecorator.TestEquals( sut.StreamDecorator ) )
            .Go();
    }

    [Fact]
    public void SetStreamDecorator_ShouldChangeValue()
    {
        MessageBrokerClientStreamDecorator value = (_, s, _) => ValueTask.FromResult<Stream>( s );
        var sut = MessageBrokerClientOptions.Default;

        var result = sut.SetStreamDecorator( value );

        Assertion.All(
                result.Tcp.TestEquals( sut.Tcp ),
                result.MinMemoryPoolSegmentLength.TestEquals( sut.MinMemoryPoolSegmentLength ),
                result.ConnectionTimeout.TestEquals( sut.ConnectionTimeout ),
                result.DesiredMessageTimeout.TestEquals( sut.DesiredMessageTimeout ),
                result.DesiredPingInterval.TestEquals( sut.DesiredPingInterval ),
                result.ListenerDisposalTimeout.TestEquals( sut.ListenerDisposalTimeout ),
                result.Timestamps.TestEquals( sut.Timestamps ),
                result.DelaySource.TestEquals( sut.DelaySource ),
                result.Logger.TestEquals( sut.Logger ),
                result.StreamDecorator.TestRefEquals( value ) )
            .Go();
    }
}
