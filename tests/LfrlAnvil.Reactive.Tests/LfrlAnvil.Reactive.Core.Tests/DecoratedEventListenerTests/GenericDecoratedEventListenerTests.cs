using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.DecoratedEventListenerTests;

public abstract class GenericDecoratedEventListenerTests<TSourceEvent, TNextEvent> : TestsBase
{
    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void OnDispose_ShouldCallNextOnDisposeByDefault(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<TNextEvent>>();
        var sut = Substitute.ForPartsOf<DecoratedEventListener<TSourceEvent, TNextEvent>>( next );

        sut.OnDispose( source );

        next.VerifyCalls().Received( x => x.OnDispose( source ) );
    }
}
