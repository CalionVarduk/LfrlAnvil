using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerTcpServerOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = MessageBrokerTcpServerOptions.Default;
        using ( new AssertionScope() )
        {
            sut.NoDelay.Should().BeNull();
            sut.SocketBufferSize.Should().BeNull();
        }
    }

    [Fact]
    public void SetNoDelay_ShouldChangeValue()
    {
        var value = true;
        var sut = MessageBrokerTcpServerOptions.Default;

        var result = sut.SetNoDelay( value );

        using ( new AssertionScope() )
        {
            result.NoDelay.Should().Be( value );
            result.SocketBufferSize.Should().Be( sut.SocketBufferSize );
        }
    }

    [Fact]
    public void SetSocketBufferSize_ShouldChangeValue()
    {
        var value = MemorySize.FromKilobytes( 16 );
        var sut = MessageBrokerTcpServerOptions.Default;

        var result = sut.SetSocketBufferSize( value );

        using ( new AssertionScope() )
        {
            result.NoDelay.Should().Be( sut.NoDelay );
            result.SocketBufferSize.Should().Be( value );
        }
    }
}
