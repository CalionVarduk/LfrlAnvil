using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class MessageBrokerListenerOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldHaveCorrectProperties()
    {
        var sut = default( MessageBrokerListenerOptions );
        Assertion.All(
                sut.MaxRetries.TestEquals( 0 ),
                sut.MaxRedeliveries.TestEquals( 0 ),
                sut.PrefetchHint.TestEquals( MessageBrokerListenerOptions.DefaultPrefetchHint ),
                sut.AreAcksEnabled.TestTrue(),
                sut.RetryDelay.TestEquals( Duration.Zero ),
                sut.DeadLetterCapacityHint.TestEquals( 0 ),
                sut.MinDeadLetterRetention.TestEquals( Duration.Zero ),
                sut.MinAckTimeout.TestEquals( MessageBrokerListenerOptions.DefaultMinAckTimeout ) )
            .Go();
    }

    [Fact]
    public void Default_ShouldCreateCorrectOptions()
    {
        var sut = MessageBrokerListenerOptions.Default;
        Assertion.All(
                sut.MaxRetries.TestEquals( 0 ),
                sut.MaxRedeliveries.TestEquals( 0 ),
                sut.PrefetchHint.TestEquals( MessageBrokerListenerOptions.DefaultPrefetchHint ),
                sut.AreAcksEnabled.TestTrue(),
                sut.RetryDelay.TestEquals( Duration.Zero ),
                sut.DeadLetterCapacityHint.TestEquals( 0 ),
                sut.MinDeadLetterRetention.TestEquals( Duration.Zero ),
                sut.MinAckTimeout.TestEquals( MessageBrokerListenerOptions.DefaultMinAckTimeout ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void SetPrefetchHint_ShouldChangePrefetchHint(short value)
    {
        var sut = MessageBrokerListenerOptions.Default;
        var result = sut.SetPrefetchHint( value );
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( value ),
                result.AreAcksEnabled.TestEquals( sut.AreAcksEnabled ),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( sut.MinAckTimeout ) )
            .Go();
    }

    [Fact]
    public void SetPrefetchHint_WithNullValue_ShouldResetPrefetchHintToDefault()
    {
        var sut = MessageBrokerListenerOptions.Default.SetPrefetchHint( 2 );
        var result = sut.SetPrefetchHint( null );
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( MessageBrokerListenerOptions.DefaultPrefetchHint ),
                result.AreAcksEnabled.TestEquals( sut.AreAcksEnabled ),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( sut.MinAckTimeout ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void SetPrefetchHint_ShouldThrowArgumentOutOfRangeException_WhenValueIsLessThanOne(short value)
    {
        var sut = MessageBrokerListenerOptions.Default;
        var action = Lambda.Of( () => sut.SetPrefetchHint( value ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0, 0, 0 )]
    [InlineData( 0, 10000, 0 )]
    [InlineData( 1, 0, 0 )]
    [InlineData( 1, 9999, 0 )]
    [InlineData( 1, 10000, 10000 )]
    [InlineData( 2, 0, 0 )]
    [InlineData( 2, 10001, 10000 )]
    [InlineData( 3, 30000, 30000 )]
    public void SetRetryPolicy_ShouldChangeMaxRetriesAndRetryDelay(int maxRetries, long retryDelayTicks, long expectedRetryDelayTicks)
    {
        var sut = MessageBrokerListenerOptions.Default;
        var result = sut.SetRetryPolicy( maxRetries, Duration.FromTicks( retryDelayTicks ) );
        Assertion.All(
                result.MaxRetries.TestEquals( maxRetries ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestEquals( sut.AreAcksEnabled ),
                result.RetryDelay.TestEquals( Duration.FromTicks( expectedRetryDelayTicks ) ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( sut.MinAckTimeout ) )
            .Go();
    }

    [Fact]
    public void SetRetryPolicy_WithNullDelay_ShouldUseDefaultRetryDelay()
    {
        var sut = MessageBrokerListenerOptions.Default;
        var result = sut.SetRetryPolicy( 1 );
        Assertion.All(
                result.MaxRetries.TestEquals( 1 ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestTrue(),
                result.RetryDelay.TestEquals( MessageBrokerListenerOptions.DefaultRetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( sut.MinAckTimeout ) )
            .Go();
    }

    [Fact]
    public void SetRetryPolicy_WithZeroMaxRetries_ShouldDisableRetryPolicy()
    {
        var sut = MessageBrokerListenerOptions.Default.SetRetryPolicy( 1 );
        var result = sut.SetRetryPolicy( 0, Duration.FromSeconds( 5 ) );
        Assertion.All(
                result.MaxRetries.TestEquals( 0 ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestEquals( sut.AreAcksEnabled ),
                result.RetryDelay.TestEquals( Duration.Zero ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( sut.MinAckTimeout ) )
            .Go();
    }

    [Fact]
    public void SetRetryPolicy_ShouldEnableAcks_WhenMaxRetriesIsGreaterThanZero()
    {
        var sut = MessageBrokerListenerOptions.Default.EnableAcks( false );
        var result = sut.SetRetryPolicy( 1 );
        Assertion.All(
                result.MaxRetries.TestEquals( 1 ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestTrue(),
                result.RetryDelay.TestEquals( MessageBrokerListenerOptions.DefaultRetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( MessageBrokerListenerOptions.DefaultMinAckTimeout ) )
            .Go();
    }

    [Fact]
    public void SetRetryPolicy_ShouldThrowArgumentOutOfRangeException_WhenMaxRetriesIsLessThanZero()
    {
        var sut = MessageBrokerListenerOptions.Default;
        var action = Lambda.Of( () => sut.SetRetryPolicy( -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( -10000 )]
    [InlineData( 21474836490000L )]
    public void SetRetryPolicy_ShouldThrowArgumentOutOfRangeException_WhenDelayIsOutOfBounds(long ticks)
    {
        var sut = MessageBrokerListenerOptions.Default;
        var action = Lambda.Of( () => sut.SetRetryPolicy( 0, Duration.FromTicks( ticks ) ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void SetMaxRedeliveries_ShouldChangeMaxRedeliveries(int value)
    {
        var sut = MessageBrokerListenerOptions.Default;
        var result = sut.SetMaxRedeliveries( value );
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( value ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestEquals( sut.AreAcksEnabled ),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( sut.MinAckTimeout ) )
            .Go();
    }

    [Fact]
    public void SetMaxRedeliveries_ShouldEnableAcks_WhenValueIsGreaterThanZero()
    {
        var sut = MessageBrokerListenerOptions.Default.EnableAcks( false );
        var result = sut.SetMaxRedeliveries( 1 );
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( 1 ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestTrue(),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( MessageBrokerListenerOptions.DefaultMinAckTimeout ) )
            .Go();
    }

    [Fact]
    public void SetMaxRedeliveries_ShouldThrowArgumentOutOfRangeException_WhenValueIsLessThanZero()
    {
        var sut = MessageBrokerListenerOptions.Default;
        var action = Lambda.Of( () => sut.SetMaxRedeliveries( -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 10000, 10000 )]
    [InlineData( 10001, 10000 )]
    [InlineData( 19999, 10000 )]
    [InlineData( 20000, 20000 )]
    [InlineData( 20001, 20000 )]
    public void SetMinAckTimeout_ShouldChangeMinAckTimeout(long ticks, long expectedTicks)
    {
        var sut = MessageBrokerListenerOptions.Default;
        var result = sut.SetMinAckTimeout( Duration.FromTicks( ticks ) );
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestEquals( sut.AreAcksEnabled ),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( Duration.FromTicks( expectedTicks ) ) )
            .Go();
    }

    [Fact]
    public void SetMinAckTimeout_WithNullValue_ShouldUseDefaultMinAckTimeout()
    {
        var sut = MessageBrokerListenerOptions.Default.SetMinAckTimeout( Duration.FromSeconds( 10 ) );
        var result = sut.SetMinAckTimeout( null );
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestEquals( sut.AreAcksEnabled ),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( MessageBrokerListenerOptions.DefaultMinAckTimeout ) )
            .Go();
    }

    [Theory]
    [InlineData( 9999 )]
    [InlineData( 21474836490000L )]
    public void SetMinAckTimeout_ShouldThrowArgumentOutOfRangeException_WhenValueIsOutOfBounds(long ticks)
    {
        var sut = MessageBrokerListenerOptions.Default;
        var action = Lambda.Of( () => sut.SetMinAckTimeout( Duration.FromTicks( ticks ) ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 1, 10000, 10000 )]
    [InlineData( 2, 10001, 10000 )]
    [InlineData( 3, 19999, 10000 )]
    [InlineData( 4, 20000, 20000 )]
    [InlineData( 5, 20001, 20000 )]
    [InlineData( 0, 10000, 0 )]
    public void SetDeadLetterPolicy_ShouldChangeDeadLetterCapacityAndRetention(
        int capacity,
        long retentionTicks,
        long expectedRetentionTicks)
    {
        var sut = MessageBrokerListenerOptions.Default;
        var result = sut.SetDeadLetterPolicy( capacity, Duration.FromTicks( retentionTicks ) );
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestEquals( sut.AreAcksEnabled ),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( capacity ),
                result.MinDeadLetterRetention.TestEquals( Duration.FromTicks( expectedRetentionTicks ) ),
                result.MinAckTimeout.TestEquals( sut.MinAckTimeout ) )
            .Go();
    }

    [Fact]
    public void SetDeadLetterPolicy_ShouldThrowArgumentOutOfRangeException_WhenCapacityIsLessThanZero()
    {
        var sut = MessageBrokerListenerOptions.Default;
        var action = Lambda.Of( () => sut.SetDeadLetterPolicy( -1, Duration.Zero ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0, -10000 )]
    [InlineData( 1, 9999 )]
    public void SetDeadLetterPolicy_ShouldThrowArgumentOutOfRangeException_WhenRetentionIsInvalid(int capacity, long retentionTicks)
    {
        var sut = MessageBrokerListenerOptions.Default;
        var action = Lambda.Of( () => sut.SetDeadLetterPolicy( capacity, Duration.FromTicks( retentionTicks ) ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void EnableAcks_ShouldAllowToDisableAcks()
    {
        var sut = MessageBrokerListenerOptions.Default;
        var result = sut.EnableAcks( false );
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestFalse(),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( Duration.Zero ) )
            .Go();
    }

    [Fact]
    public void EnableAcks_ShouldAllowToEnableAcks()
    {
        var sut = MessageBrokerListenerOptions.Default.EnableAcks( false );
        var result = sut.EnableAcks();
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestTrue(),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( sut.DeadLetterCapacityHint ),
                result.MinDeadLetterRetention.TestEquals( sut.MinDeadLetterRetention ),
                result.MinAckTimeout.TestEquals( MessageBrokerListenerOptions.DefaultMinAckTimeout ) )
            .Go();
    }

    [Theory]
    [InlineData( 0, 1, 0 )]
    [InlineData( 1, 0, 0 )]
    [InlineData( 0, 0, 1 )]
    [InlineData( 1, 1, 1 )]
    public void EnableAcks_ShouldNotDisableAcks_WhenMaxRetriesOrMaxRedeliveriesOrDeadLetterIsEnabled(
        int maxRetries,
        int maxRedeliveries,
        int deadLetterCapacity)
    {
        var sut = MessageBrokerListenerOptions.Default
            .SetRetryPolicy( maxRetries )
            .SetMaxRedeliveries( maxRedeliveries )
            .SetDeadLetterPolicy( deadLetterCapacity, Duration.FromHours( 1 ) );

        var result = sut.EnableAcks( false );
        Assertion.All(
                result.MaxRetries.TestEquals( sut.MaxRetries ),
                result.MaxRedeliveries.TestEquals( sut.MaxRedeliveries ),
                result.PrefetchHint.TestEquals( sut.PrefetchHint ),
                result.AreAcksEnabled.TestTrue(),
                result.RetryDelay.TestEquals( sut.RetryDelay ),
                result.DeadLetterCapacityHint.TestEquals( deadLetterCapacity ),
                result.MinDeadLetterRetention.TestEquals( deadLetterCapacity > 0 ? Duration.FromHours( 1 ) : Duration.Zero ),
                result.MinAckTimeout.TestEquals( sut.MinAckTimeout ) )
            .Go();
    }
}
