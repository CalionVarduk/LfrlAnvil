using System;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Tests.LazyDisposableTests
{
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
                inner.DidNotReceive().Dispose();
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
                inner.Received().Dispose();
            }
        }

        [Fact]
        public void Assign_ShouldThrowLazyDisposableAssignmentException_WhenInnerDisposableIsNotNull()
        {
            var inner = Substitute.For<IDisposable>();
            var sut = new LazyDisposable<IDisposable>();
            sut.Assign( inner );

            var action = Lambda.Of( () => sut.Assign( inner ) );

            action.Should().ThrowExactly<LazyDisposableAssignmentException>();
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
                inner.Received().Dispose();
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
                inner.Received( 1 ).Dispose();
            }
        }
    }
}
