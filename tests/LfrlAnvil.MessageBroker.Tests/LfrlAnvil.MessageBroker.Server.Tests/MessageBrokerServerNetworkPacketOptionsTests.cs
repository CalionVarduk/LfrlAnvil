using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerServerNetworkPacketOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = MessageBrokerServerNetworkPacketOptions.Default;
        Assertion.All(
                sut.MaxLength.TestNull(),
                sut.MaxMessageLength.TestNull(),
                sut.MaxBatchLength.TestNull(),
                sut.MaxBatchPacketCount.TestNull() )
            .Go();
    }

    [Fact]
    public void SetMaxLength_ShouldChangeValue()
    {
        var value = MemorySize.FromKilobytes( 32 );
        var sut = MessageBrokerServerNetworkPacketOptions.Default;

        var result = sut.SetMaxLength( value );

        Assertion.All(
                result.MaxLength.TestEquals( value ),
                result.MaxMessageLength.TestEquals( sut.MaxMessageLength ),
                result.MaxBatchLength.TestEquals( sut.MaxBatchLength ),
                result.MaxBatchPacketCount.TestEquals( sut.MaxBatchPacketCount ) )
            .Go();
    }

    [Fact]
    public void SetMaxMessageLength_ShouldChangeValue()
    {
        var value = MemorySize.FromMegabytes( 100 );
        var sut = MessageBrokerServerNetworkPacketOptions.Default;

        var result = sut.SetMaxMessageLength( value );

        Assertion.All(
                result.MaxLength.TestEquals( sut.MaxLength ),
                result.MaxMessageLength.TestEquals( value ),
                result.MaxBatchLength.TestEquals( sut.MaxBatchLength ),
                result.MaxBatchPacketCount.TestEquals( sut.MaxBatchPacketCount ) )
            .Go();
    }

    [Fact]
    public void SetMaxBatchLength_ShouldChangeValue()
    {
        var value = MemorySize.FromMegabytes( 50 );
        var sut = MessageBrokerServerNetworkPacketOptions.Default;

        var result = sut.SetMaxBatchLength( value );

        Assertion.All(
                result.MaxLength.TestEquals( sut.MaxLength ),
                result.MaxMessageLength.TestEquals( sut.MaxMessageLength ),
                result.MaxBatchLength.TestEquals( value ),
                result.MaxBatchPacketCount.TestEquals( sut.MaxBatchPacketCount ) )
            .Go();
    }

    [Fact]
    public void SetMaxBatchPacketCount_ShouldChangeValue()
    {
        var value = ( short )100;
        var sut = MessageBrokerServerNetworkPacketOptions.Default;

        var result = sut.SetMaxBatchPacketCount( value );

        Assertion.All(
                result.MaxLength.TestEquals( sut.MaxLength ),
                result.MaxMessageLength.TestEquals( sut.MaxMessageLength ),
                result.MaxBatchLength.TestEquals( sut.MaxBatchLength ),
                result.MaxBatchPacketCount.TestEquals( value ) )
            .Go();
    }
}
