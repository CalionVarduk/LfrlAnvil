using System.Threading;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.DiagnosticsTests;

public class MeasureTests : TestsBase
{
    [Fact]
    public void Call_WithAction_ShouldReturnElapsedTime()
    {
        var callCount = 0;
        var action = Lambda.Of(
            () =>
            {
                ++callCount;
                Thread.Sleep( 1 );
            } );

        var result = Measure.Call( action );

        using ( new AssertionScope() )
        {
            callCount.Should().Be( 1 );
            result.Should().BeGreaterThan( TimeSpan.Zero );
        }
    }

    [Fact]
    public void Call_WithFunc_ShouldReturnElapsedTime()
    {
        var callCount = 0;
        var action = Lambda.Of(
            () =>
            {
                ++callCount;
                Thread.Sleep( 1 );
                return "foo";
            } );

        var result = Measure.Call( action );

        using ( new AssertionScope() )
        {
            callCount.Should().Be( 1 );
            result.Result.Should().Be( "foo" );
            result.ElapsedTime.Should().BeGreaterThan( TimeSpan.Zero );
        }
    }
}
