using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests;

public class SqlDatabaseConnectionChangeCallbacksTests : TestsBase
{
    [Fact]
    public void Create_ShouldCreateEmptyCallbacksList()
    {
        var sut = SqlDatabaseConnectionChangeCallbacks.Create();

        using ( new AssertionScope() )
        {
            sut.Callbacks.Should().BeEmpty();
            sut.FirstPendingCallbackIndex.Should().Be( 0 );
        }
    }

    [Fact]
    public void UpdateFirstPendingCallbackIndex_ShouldDoNothing_WhenFirstPendingCallbackIndexDidNotChangeAndCallbacksAreEmpty()
    {
        var sut = SqlDatabaseConnectionChangeCallbacks.Create();
        var result = sut.UpdateFirstPendingCallbackIndex();

        using ( new AssertionScope() )
        {
            result.Callbacks.Should().BeSameAs( sut.Callbacks );
            result.FirstPendingCallbackIndex.Should().Be( 0 );
        }
    }

    [Fact]
    public void UpdateFirstPendingCallbackIndex_ShouldChangeFirstPendingCallbackIndexToAmountOfRegisteredCallbacks()
    {
        var c1 = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();
        var c2 = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();

        var sut = SqlDatabaseConnectionChangeCallbacks.Create();
        sut.AddCallback( c1 );
        sut.AddCallback( c2 );

        var result = sut.UpdateFirstPendingCallbackIndex();

        using ( new AssertionScope() )
        {
            result.Callbacks.Should().BeSameAs( sut.Callbacks );
            result.FirstPendingCallbackIndex.Should().Be( 2 );
        }
    }

    [Fact]
    public void GetPendingCallbacks_ShouldReturnEmptyCollection_WhenCallbacksAreEmpty()
    {
        var sut = SqlDatabaseConnectionChangeCallbacks.Create();
        var result = sut.GetPendingCallbacks();
        result.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void GetPendingCallbacks_ShouldReturnAllCallbacks_WhenFirstPendingCallbackIndexDidNotChange()
    {
        var c1 = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();
        var c2 = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();

        var sut = SqlDatabaseConnectionChangeCallbacks.Create();
        sut.AddCallback( c1 );
        sut.AddCallback( c2 );

        var result = sut.GetPendingCallbacks();

        result.ToArray().Should().BeSequentiallyEqualTo( sut.Callbacks );
    }

    [Fact]
    public void GetPendingCallbacks_ShouldReturnAllCallbacks_WhenFirstPendingCallbackIndexChanged()
    {
        var c1 = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();
        var c2 = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();
        var c3 = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();
        var c4 = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();

        var sut = SqlDatabaseConnectionChangeCallbacks.Create();
        sut.AddCallback( c1 );
        sut.AddCallback( c2 );
        sut = sut.UpdateFirstPendingCallbackIndex();
        sut.AddCallback( c3 );
        sut.AddCallback( c4 );

        var result = sut.GetPendingCallbacks();

        result.ToArray().Should().BeSequentiallyEqualTo( c3, c4 );
    }
}
