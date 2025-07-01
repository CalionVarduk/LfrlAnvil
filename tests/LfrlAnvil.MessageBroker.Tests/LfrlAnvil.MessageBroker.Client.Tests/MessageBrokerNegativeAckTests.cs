using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerNegativeAckTests : TestsBase
{
    [Fact]
    public void Default_ShouldHaveCorrectProperties()
    {
        var sut = default( MessageBrokerNegativeAck );
        Assertion.All( sut.SkipRetry.TestFalse(), sut.RetryDelay.TestNull() ).Go();
    }

    [Fact]
    public void Default_ShouldCreateCorrectNack()
    {
        var sut = MessageBrokerNegativeAck.Default;
        Assertion.All( sut.SkipRetry.TestFalse(), sut.RetryDelay.TestNull() ).Go();
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 0 )]
    [InlineData( 9999, 0 )]
    [InlineData( 10000, 10000 )]
    [InlineData( 10001, 10000 )]
    public void Retry_ShouldCreateCorrectNack(long ticks, long expectedTicks)
    {
        var sut = MessageBrokerNegativeAck.Retry( Duration.FromTicks( ticks ) );
        Assertion.All( sut.SkipRetry.TestFalse(), sut.RetryDelay.TestEquals( Duration.FromTicks( expectedTicks ) ) ).Go();
    }

    [Theory]
    [InlineData( -10000 )]
    [InlineData( 21474836490000L )]
    public void Retry_ShouldThrowArgumentOutOfRangeException_WhenDelayIsOutOfBounds(long ticks)
    {
        var action = Lambda.Of( () => MessageBrokerNegativeAck.Retry( Duration.FromTicks( ticks ) ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void NoRetry_ShouldCreateCorrectNack()
    {
        var sut = MessageBrokerNegativeAck.NoRetry();
        Assertion.All( sut.SkipRetry.TestTrue(), sut.RetryDelay.TestNull() ).Go();
    }
}
