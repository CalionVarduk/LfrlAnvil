using System.Net;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerTcpClientOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = MessageBrokerTcpClientOptions.Default;
        Assertion.All(
                sut.LocalEndPoint.TestNull(),
                sut.NoDelay.TestNull(),
                sut.SocketBufferSize.TestNull() )
            .Go();
    }

    [Fact]
    public void SetLocalEndPoint_ShouldChangeValue()
    {
        var value = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = MessageBrokerTcpClientOptions.Default;

        var result = sut.SetLocalEndPoint( value );

        Assertion.All(
                result.LocalEndPoint.TestRefEquals( value ),
                result.NoDelay.TestEquals( sut.NoDelay ),
                result.SocketBufferSize.TestEquals( sut.SocketBufferSize ) )
            .Go();
    }

    [Fact]
    public void SetNoDelay_ShouldChangeValue()
    {
        var value = true;
        var sut = MessageBrokerTcpClientOptions.Default;

        var result = sut.SetNoDelay( value );

        Assertion.All(
                result.LocalEndPoint.TestEquals( sut.LocalEndPoint ),
                result.NoDelay.TestEquals( value ),
                result.SocketBufferSize.TestEquals( sut.SocketBufferSize ) )
            .Go();
    }

    [Fact]
    public void SetSocketBufferSize_ShouldChangeValue()
    {
        var value = MemorySize.FromKilobytes( 16 );
        var sut = MessageBrokerTcpClientOptions.Default;

        var result = sut.SetSocketBufferSize( value );

        Assertion.All(
                result.LocalEndPoint.TestEquals( sut.LocalEndPoint ),
                result.NoDelay.TestEquals( sut.NoDelay ),
                result.SocketBufferSize.TestEquals( value ) )
            .Go();
    }
}
