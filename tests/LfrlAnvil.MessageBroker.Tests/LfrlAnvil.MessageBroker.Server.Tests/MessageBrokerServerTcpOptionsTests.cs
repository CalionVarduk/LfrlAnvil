using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerServerTcpOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = MessageBrokerServerTcpOptions.Default;
        Assertion.All(
                sut.NoDelay.TestNull(),
                sut.SocketBufferSize.TestNull() )
            .Go();
    }

    [Fact]
    public void SetNoDelay_ShouldChangeValue()
    {
        var value = true;
        var sut = MessageBrokerServerTcpOptions.Default;

        var result = sut.SetNoDelay( value );

        Assertion.All(
                result.NoDelay.TestEquals( value ),
                result.SocketBufferSize.TestEquals( sut.SocketBufferSize ) )
            .Go();
    }

    [Fact]
    public void SetSocketBufferSize_ShouldChangeValue()
    {
        var value = MemorySize.FromKilobytes( 16 );
        var sut = MessageBrokerServerTcpOptions.Default;

        var result = sut.SetSocketBufferSize( value );

        Assertion.All(
                result.NoDelay.TestEquals( sut.NoDelay ),
                result.SocketBufferSize.TestEquals( value ) )
            .Go();
    }
}
