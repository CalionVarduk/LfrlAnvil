using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.LazyDisposableTests;

public class LazyDisposableTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateNotDisposedAndWithoutInnerDisposable()
    {
        var sut = new LazyDisposable<IDisposable>();

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeFalse();
            sut.CanAssign.Should().BeTrue();
            sut.Inner.Should().BeNull();
        }
    }

    [Fact]
    public void Assign_ShouldLinkInnerDisposable_WhenNotDisposedAndCanAssign()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();

        sut.Assign( inner );

        using ( new AssertionScope() )
        {
            sut.Inner.Should().BeSameAs( inner );
            sut.IsDisposed.Should().BeFalse();
            sut.CanAssign.Should().BeFalse();
            inner.VerifyCalls().DidNotReceive( x => x.Dispose() );
        }
    }

    [Fact]
    public void Assign_ShouldLinkInnerDisposableAndDisposeIt_WhenDisposeHasBeenCalledPreviouslyAndCanAssign()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();
        sut.Dispose();

        sut.Assign( inner );

        using ( new AssertionScope() )
        {
            sut.Inner.Should().BeSameAs( inner );
            sut.IsDisposed.Should().BeTrue();
            sut.CanAssign.Should().BeFalse();
            inner.VerifyCalls().Received( x => x.Dispose() );
        }
    }

    [Fact]
    public void Assign_ShouldThrowInvalidOperationException_WhenInnerDisposableIsNotNull()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();
        sut.Assign( inner );

        var action = Lambda.Of( () => sut.Assign( inner ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Dispose_ShouldMarkAsDisposed_WhenInnerDisposableIsNull()
    {
        var sut = new LazyDisposable<IDisposable>();
        sut.Dispose();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldDisposeInnerDisposable_WhenInnerDisposableIsNotNull()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();
        sut.Assign( inner );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeTrue();
            inner.VerifyCalls().Received( x => x.Dispose() );
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenAlreadyDisposed()
    {
        var inner = Substitute.For<IDisposable>();
        var sut = new LazyDisposable<IDisposable>();
        sut.Assign( inner );
        sut.Dispose();

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeTrue();
            inner.VerifyCalls().Received( x => x.Dispose(), count: 1 );
        }
    }
}
