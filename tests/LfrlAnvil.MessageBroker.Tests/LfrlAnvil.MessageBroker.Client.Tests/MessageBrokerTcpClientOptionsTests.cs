using System.Net;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerTcpClientOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = MessageBrokerTcpClientOptions.Default;
        using ( new AssertionScope() )
        {
            sut.LocalEndPoint.Should().BeNull();
            sut.NoDelay.Should().BeNull();
            sut.SocketBufferSize.Should().BeNull();
        }
    }

    [Fact]
    public void SetLocalEndPoint_ShouldChangeValue()
    {
        var value = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = MessageBrokerTcpClientOptions.Default;

        var result = sut.SetLocalEndPoint( value );

        using ( new AssertionScope() )
        {
            result.LocalEndPoint.Should().BeSameAs( value );
            result.NoDelay.Should().Be( sut.NoDelay );
            result.SocketBufferSize.Should().Be( sut.SocketBufferSize );
        }
    }

    [Fact]
    public void SetNoDelay_ShouldChangeValue()
    {
        var value = true;
        var sut = MessageBrokerTcpClientOptions.Default;

        var result = sut.SetNoDelay( value );

        using ( new AssertionScope() )
        {
            result.LocalEndPoint.Should().Be( sut.LocalEndPoint );
            result.NoDelay.Should().Be( value );
            result.SocketBufferSize.Should().Be( sut.SocketBufferSize );
        }
    }

    [Fact]
    public void SetSocketBufferSize_ShouldChangeValue()
    {
        var value = MemorySize.FromKilobytes( 16 );
        var sut = MessageBrokerTcpClientOptions.Default;

        var result = sut.SetSocketBufferSize( value );

        using ( new AssertionScope() )
        {
            result.LocalEndPoint.Should().Be( sut.LocalEndPoint );
            result.NoDelay.Should().Be( sut.NoDelay );
            result.SocketBufferSize.Should().Be( value );
        }
    }
}
