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

        using ( new AssertionScope() )
        {
            sut.PreviousContext.Should().BeSameAs( previousContext );
            sut.Context.Should().BeSameAs( context );
            SynchronizationContext.Current.Should().BeSameAs( context );
        }
    }

    [Fact]
    public void Dispose_ShouldRevertContextSwitch()
    {
        var previousContext = SynchronizationContext.Current;
        var context = new SynchronizationContext();
        var sut = new SynchronizationContextSwitch( context );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.PreviousContext.Should().BeSameAs( previousContext );
            sut.Context.Should().BeSameAs( context );
            SynchronizationContext.Current.Should().BeSameAs( previousContext );
        }
    }
}
