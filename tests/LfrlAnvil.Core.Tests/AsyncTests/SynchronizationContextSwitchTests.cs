using System.Threading;
using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public class SynchronizationContextSwitchTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldOverrideCurrentContextWithProvidedContext()
    {
        var previousContext = SynchronizationContext.Current;
        var context = new SynchronizationContext();
        var sut = new SynchronizationContextSwitch( context );

        Assertion.All(
                sut.PreviousContext.TestRefEquals( previousContext ),
                sut.Context.TestRefEquals( context ),
                SynchronizationContext.Current.TestRefEquals( context ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldRevertContextSwitch()
    {
        var previousContext = SynchronizationContext.Current;
        var context = new SynchronizationContext();
        var sut = new SynchronizationContextSwitch( context );

        sut.Dispose();

        Assertion.All(
                sut.PreviousContext.TestRefEquals( previousContext ),
                sut.Context.TestRefEquals( context ),
                SynchronizationContext.Current.TestRefEquals( previousContext ) )
            .Go();
    }
}
