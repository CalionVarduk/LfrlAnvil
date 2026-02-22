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
        var action = Lambda.Of( () =>
        {
            ++callCount;
            Thread.Sleep( 1 );
        } );

        var result = Measure.Call( action );

        Assertion.All(
                callCount.TestEquals( 1 ),
                result.TestGreaterThan( TimeSpan.Zero ) )
            .Go();
    }

    [Fact]
    public void Call_WithFunc_ShouldReturnElapsedTime()
    {
        var callCount = 0;
        var action = Lambda.Of( () =>
        {
            ++callCount;
            Thread.Sleep( 1 );
            return "foo";
        } );

        var result = Measure.Call( action );

        Assertion.All(
                callCount.TestEquals( 1 ),
                result.Result.TestEquals( "foo" ),
                result.ElapsedTime.TestGreaterThan( TimeSpan.Zero ) )
            .Go();
    }
}
