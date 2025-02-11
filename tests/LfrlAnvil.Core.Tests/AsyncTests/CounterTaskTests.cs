using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class CounterTaskTests : TestsBase
{
    [Theory]
    [InlineData( 14, 0, 14 )]
    [InlineData( 14, -1, 14 )]
    [InlineData( 0, -1, 0 )]
    [InlineData( -1, -1, 0 )]
    public void Ctor_ShouldCreateTaskInProgress(int limit, int count, int expectedLimit)
    {
        var sut = new CounterTask( limit, count );
        Assertion.All(
                sut.Limit.TestEquals( expectedLimit ),
                sut.Count.TestEquals( count ),
                sut.Task.IsCompleted.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( 14 )]
    [InlineData( 15 )]
    public void Ctor_ShouldCreateCompletedTask_WhenCountIsGreaterThanOrEqualToLimit(int count)
    {
        var sut = new CounterTask( limit: 14, count );
        Assertion.All(
                sut.Limit.TestEquals( 14 ),
                sut.Count.TestEquals( 14 ),
                sut.Task.IsCompletedSuccessfully.TestTrue() )
            .Go();
    }

    [Fact]
    public void Increment_ShouldIncrementCountAndReturnFalse_WhenNewCountIsLessThanLimit()
    {
        var sut = new CounterTask( limit: 2 );

        var result = sut.Increment();

        Assertion.All(
                result.TestFalse(),
                sut.Limit.TestEquals( 2 ),
                sut.Count.TestEquals( 1 ),
                sut.Task.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public void Increment_ShouldIncrementCountAndReturnTrueAndCompleteTask_WhenNewCountReachesLimit()
    {
        var sut = new CounterTask( limit: 2, count: 1 );

        var result = sut.Increment();

        Assertion.All(
                result.TestTrue(),
                sut.Limit.TestEquals( 2 ),
                sut.Count.TestEquals( 2 ),
                sut.Task.IsCompletedSuccessfully.TestTrue() )
            .Go();
    }

    [Fact]
    public void Increment_ShouldNotIncrementCountAndReturnTrue_WhenTaskIsCompleted()
    {
        var sut = new CounterTask( limit: 2, count: 2 );

        var result = sut.Increment();

        Assertion.All(
                result.TestTrue(),
                sut.Limit.TestEquals( 2 ),
                sut.Count.TestEquals( 2 ),
                sut.Task.IsCompletedSuccessfully.TestTrue() )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddCountAndReturnFalse_WhenNewCountIsLessThanLimit()
    {
        var sut = new CounterTask( limit: 3 );

        var result = sut.Add( 2 );

        Assertion.All(
                result.TestFalse(),
                sut.Limit.TestEquals( 3 ),
                sut.Count.TestEquals( 2 ),
                sut.Task.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddCountAndReturnTrueAndCompleteTask_WhenNewCountReachesLimit()
    {
        var sut = new CounterTask( limit: 3, count: 1 );

        var result = sut.Add( 3 );

        Assertion.All(
                result.TestTrue(),
                sut.Limit.TestEquals( 3 ),
                sut.Count.TestEquals( 3 ),
                sut.Task.IsCompletedSuccessfully.TestTrue() )
            .Go();
    }

    [Fact]
    public void Add_ShouldNotAddCountAndReturnTrue_WhenTaskIsCompleted()
    {
        var sut = new CounterTask( limit: 2, count: 2 );

        var result = sut.Add( 1 );

        Assertion.All(
                result.TestTrue(),
                sut.Limit.TestEquals( 2 ),
                sut.Count.TestEquals( 2 ),
                sut.Task.IsCompletedSuccessfully.TestTrue() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Add_ShouldDoNothing_WhenValueIsLessThanOne(int value)
    {
        var sut = new CounterTask( limit: 1 );

        var result = sut.Add( value );

        Assertion.All(
                result.TestFalse(),
                sut.Limit.TestEquals( 1 ),
                sut.Count.TestEquals( 0 ),
                sut.Task.IsCompleted.TestFalse() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldCancelTask()
    {
        var sut = new CounterTask( limit: 1 );

        sut.Dispose();

        Assertion.All(
                sut.Limit.TestEquals( 1 ),
                sut.Count.TestEquals( 0 ),
                sut.Task.IsCanceled.TestTrue() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenTaskIsAlreadyCancelled()
    {
        var sut = new CounterTask( limit: 1 );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }
}
