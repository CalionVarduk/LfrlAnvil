using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerNegativeAckTests : TestsBase
{
    [Fact]
    public void Default_ShouldHaveCorrectProperties()
    {
        var sut = default( MessageBrokerNegativeAck );
        Assertion.All( sut.SkipRetry.TestFalse(), sut.SkipDeadLetter.TestFalse(), sut.RetryDelay.TestNull() ).Go();
    }

    [Fact]
    public void Default_ShouldCreateCorrectNack()
    {
        var sut = MessageBrokerNegativeAck.Default;
        Assertion.All( sut.SkipRetry.TestFalse(), sut.SkipDeadLetter.TestFalse(), sut.RetryDelay.TestNull() ).Go();
    }

    [Theory]
    [InlineData( 0, true, 0 )]
    [InlineData( 1, false, 0 )]
    [InlineData( 9999, true, 0 )]
    [InlineData( 10000, false, 10000 )]
    [InlineData( 10001, true, 10000 )]
    public void Retry_ShouldCreateCorrectNack(long ticks, bool skipDeadLetter, long expectedTicks)
    {
        var sut = MessageBrokerNegativeAck.Retry( Duration.FromTicks( ticks ), skipDeadLetter );
        Assertion.All(
                sut.SkipRetry.TestFalse(),
                sut.SkipDeadLetter.TestEquals( skipDeadLetter ),
                sut.RetryDelay.TestEquals( Duration.FromTicks( expectedTicks ) ) )
            .Go();
    }

    [Theory]
    [InlineData( -10000 )]
    [InlineData( 21474836490000L )]
    public void Retry_ShouldThrowArgumentOutOfRangeException_WhenDelayIsOutOfBounds(long ticks)
    {
        var action = Lambda.Of( () => MessageBrokerNegativeAck.Retry( Duration.FromTicks( ticks ) ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void NoRetry_ShouldCreateCorrectNack(bool skipDeadLetter)
    {
        var sut = MessageBrokerNegativeAck.NoRetry( skipDeadLetter );
        Assertion.All( sut.SkipRetry.TestTrue(), sut.SkipDeadLetter.TestEquals( skipDeadLetter ), sut.RetryDelay.TestNull() ).Go();
    }

    [Fact]
    public void NoDeadLetter_ShouldCreateCorrectNack()
    {
        var sut = MessageBrokerNegativeAck.NoDeadLetter();
        Assertion.All( sut.SkipRetry.TestFalse(), sut.SkipDeadLetter.TestTrue(), sut.RetryDelay.TestNull() ).Go();
    }
}
