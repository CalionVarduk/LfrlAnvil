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
        using ( new AssertionScope() )
        {
            sut.Limit.Should().Be( expectedLimit );
            sut.Count.Should().Be( count );
            sut.Task.IsCompleted.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( 14 )]
    [InlineData( 15 )]
    public void Ctor_ShouldCreateCompletedTask_WhenCountIsGreaterThanOrEqualToLimit(int count)
    {
        var sut = new CounterTask( limit: 14, count );
        using ( new AssertionScope() )
        {
            sut.Limit.Should().Be( 14 );
            sut.Count.Should().Be( 14 );
            sut.Task.IsCompletedSuccessfully.Should().BeTrue();
        }
    }

    [Fact]
    public void Increment_ShouldIncrementCountAndReturnFalse_WhenNewCountIsLessThanLimit()
    {
        var sut = new CounterTask( limit: 2 );

        var result = sut.Increment();

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Limit.Should().Be( 2 );
            sut.Count.Should().Be( 1 );
            sut.Task.IsCompleted.Should().BeFalse();
        }
    }

    [Fact]
    public void Increment_ShouldIncrementCountAndReturnTrueAndCompleteTask_WhenNewCountReachesLimit()
    {
        var sut = new CounterTask( limit: 2, count: 1 );

        var result = sut.Increment();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Limit.Should().Be( 2 );
            sut.Count.Should().Be( 2 );
            sut.Task.IsCompletedSuccessfully.Should().BeTrue();
        }
    }

    [Fact]
    public void Increment_ShouldNotIncrementCountAndReturnTrue_WhenTaskIsCompleted()
    {
        var sut = new CounterTask( limit: 2, count: 2 );

        var result = sut.Increment();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Limit.Should().Be( 2 );
            sut.Count.Should().Be( 2 );
            sut.Task.IsCompletedSuccessfully.Should().BeTrue();
        }
    }

    [Fact]
    public void Add_ShouldAddCountAndReturnFalse_WhenNewCountIsLessThanLimit()
    {
        var sut = new CounterTask( limit: 3 );

        var result = sut.Add( 2 );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Limit.Should().Be( 3 );
            sut.Count.Should().Be( 2 );
            sut.Task.IsCompleted.Should().BeFalse();
        }
    }

    [Fact]
    public void Add_ShouldAddCountAndReturnTrueAndCompleteTask_WhenNewCountReachesLimit()
    {
        var sut = new CounterTask( limit: 3, count: 1 );

        var result = sut.Add( 3 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Limit.Should().Be( 3 );
            sut.Count.Should().Be( 3 );
            sut.Task.IsCompletedSuccessfully.Should().BeTrue();
        }
    }

    [Fact]
    public void Add_ShouldNotAddCountAndReturnTrue_WhenTaskIsCompleted()
    {
        var sut = new CounterTask( limit: 2, count: 2 );

        var result = sut.Add( 1 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Limit.Should().Be( 2 );
            sut.Count.Should().Be( 2 );
            sut.Task.IsCompletedSuccessfully.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Add_ShouldDoNothing_WhenValueIsLessThanOne(int value)
    {
        var sut = new CounterTask( limit: 1 );

        var result = sut.Add( value );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Limit.Should().Be( 1 );
            sut.Count.Should().Be( 0 );
            sut.Task.IsCompleted.Should().BeFalse();
        }
    }

    [Fact]
    public void Dispose_ShouldCancelTask()
    {
        var sut = new CounterTask( limit: 1 );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.Limit.Should().Be( 1 );
            sut.Count.Should().Be( 0 );
            sut.Task.IsCanceled.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenTaskIsAlreadyCancelled()
    {
        var sut = new CounterTask( limit: 1 );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }
}
