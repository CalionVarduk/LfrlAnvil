using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.LazyDisposableTests;

public class LazyDisposableTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateNotDisposedAndWithoutInnerDisposable()
    {
        var sut = new LazyDisposable<IDisposable>();

        Assertion.All(
                sut.IsDisposed.TestFalse(),
                sut.CanAssign.TestTrue(),
                sut.Inner.TestNull() )
            .Go();
    }

    [Fact]
    public void Assign_ShouldLinkInnerDisposable_WhenNotDisposedAndCanAssign()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();

        sut.Assign( inner );

        Assertion.All(
                sut.Inner.TestRefEquals( inner ),
                sut.IsDisposed.TestFalse(),
                sut.CanAssign.TestFalse(),
                inner.TestDidNotReceiveCall( x => x.Dispose() ) )
            .Go();
    }

    [Fact]
    public void Assign_ShouldLinkInnerDisposableAndDisposeIt_WhenDisposeHasBeenCalledPreviouslyAndCanAssign()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();
        sut.Dispose();

        sut.Assign( inner );

        Assertion.All(
                sut.Inner.TestRefEquals( inner ),
                sut.IsDisposed.TestTrue(),
                sut.CanAssign.TestFalse(),
                inner.TestReceivedCall( x => x.Dispose() ) )
            .Go();
    }

    [Fact]
    public void Assign_ShouldThrowInvalidOperationException_WhenInnerDisposableIsNotNull()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();
        sut.Assign( inner );

        var action = Lambda.Of( () => sut.Assign( inner ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Dispose_ShouldMarkAsDisposed_WhenInnerDisposableIsNull()
    {
        var sut = new LazyDisposable<IDisposable>();
        sut.Dispose();
        sut.IsDisposed.TestTrue().Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeInnerDisposable_WhenInnerDisposableIsNotNull()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();
        sut.Assign( inner );

        sut.Dispose();

        Assertion.All(
                sut.IsDisposed.TestTrue(),
                inner.TestReceivedCall( x => x.Dispose() ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenAlreadyDisposed()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();
        sut.Assign( inner );
        sut.Dispose();

        sut.Dispose();

        Assertion.All(
                sut.IsDisposed.TestTrue(),
                inner.TestReceivedCalls( x => x.Dispose(), count: 1 ) )
            .Go();
    }
}
