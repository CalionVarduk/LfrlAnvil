using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Events;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventsTests.LazyEventSubscriberTests
{
    public class LazyEventSubscriberTests : TestsBase
    {
        [Fact]
        public void Ctor_ShouldCreateNotDisposedAndWithoutInternalSubscriber()
        {
            var sut = new LazyEventSubscriber();

            using ( new AssertionScope() )
            {
                sut.IsDisposed.Should().BeFalse();
                sut.Subscriber.Should().BeNull();
            }
        }

        [Fact]
        public void Initialize_ShouldLinkInternalSubscriber_WhenNotDisposedAndInternalSubscriberIsNull()
        {
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new LazyEventSubscriber();

            sut.Initialize( subscriber );

            using ( new AssertionScope() )
            {
                sut.Subscriber.Should().BeSameAs( subscriber );
                subscriber.DidNotReceive().Dispose();
            }
        }

        [Fact]
        public void Initialize_ShouldLinkInternalSubscriberAndDisposeIt_WhenDisposeHasBeenCalledPreviouslyAndInternalSubscriberIsNull()
        {
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new LazyEventSubscriber();
            sut.Dispose();

            sut.Initialize( subscriber );

            using ( new AssertionScope() )
            {
                sut.Subscriber.Should().BeSameAs( subscriber );
                subscriber.Received().Dispose();
            }
        }

        [Fact]
        public void Initialize_ShouldThrowSubscriberInitializationException_WhenInternalSubscriberIsNotNull()
        {
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new LazyEventSubscriber();
            sut.Initialize( subscriber );

            var action = Lambda.Of( () => sut.Initialize( subscriber ) );

            action.Should().ThrowExactly<SubscriberInitializationException>();
        }

        [Fact]
        public void Dispose_ShouldMarkSubscriberAsDisposed_WhenInternalSubscriberIsNull()
        {
            var sut = new LazyEventSubscriber();
            sut.Dispose();
            sut.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public void Dispose_ShouldDisposeInternalSubscriber_WhenInternalSubscriberIsNotNull()
        {
            var isDisposed = false;
            var subscriber = Substitute.For<IEventSubscriber>();
            subscriber.IsDisposed.Returns( _ => isDisposed );
            subscriber.When( x => x.Dispose() ).Do( _ => isDisposed = true );

            var sut = new LazyEventSubscriber();
            sut.Initialize( subscriber );

            sut.Dispose();

            using ( new AssertionScope() )
            {
                sut.IsDisposed.Should().BeTrue();
                subscriber.Received().Dispose();
            }
        }
    }
}
