using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerClientNetworkPacketOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = MessageBrokerClientNetworkPacketOptions.Default;
        Assertion.All(
                sut.DesiredMaxBatchLength.TestNull(),
                sut.DesiredMaxBatchPacketCount.TestNull() )
            .Go();
    }

    [Fact]
    public void SetDesiredMaxBatchLength_ShouldChangeValue()
    {
        var value = MemorySize.FromMegabytes( 50 );
        var sut = MessageBrokerClientNetworkPacketOptions.Default;

        var result = sut.SetDesiredMaxBatchLength( value );

        Assertion.All(
                result.DesiredMaxBatchLength.TestEquals( value ),
                result.DesiredMaxBatchPacketCount.TestEquals( sut.DesiredMaxBatchPacketCount ) )
            .Go();
    }

    [Fact]
    public void SetDesiredMaxBatchPacketCount_ShouldChangeValue()
    {
        var value = ( short )100;
        var sut = MessageBrokerClientNetworkPacketOptions.Default;

        var result = sut.SetDesiredMaxBatchPacketCount( value );

        Assertion.All(
                result.DesiredMaxBatchLength.TestEquals( sut.DesiredMaxBatchLength ),
                result.DesiredMaxBatchPacketCount.TestEquals( value ) )
            .Go();
    }
}
