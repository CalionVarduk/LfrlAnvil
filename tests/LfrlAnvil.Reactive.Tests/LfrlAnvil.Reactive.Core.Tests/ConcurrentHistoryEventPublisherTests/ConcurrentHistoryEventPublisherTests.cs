using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.Reactive.Internal;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.ConcurrentHistoryEventPublisherTests
{
    public class ConcurrentHistoryEventPublisherTests : TestsBase
    {
        [Theory]
        [InlineData( 1 )]
        [InlineData( 5 )]
        [InlineData( 10 )]
        public void Capacity_ShouldBeEquivalentToBaseEventPublisherCapacity(int capacity)
        {
            var @base = new HistoryEventPublisher<int>( capacity );
            var sut = new ConcurrentHistoryEventPublisher<int, HistoryEventPublisher<int>>( @base );
            sut.Capacity.Should().Be( @base.Capacity );
        }

        [Fact]
        public void History_ShouldBeEquivalentToBaseEventPublisherHistory()
        {
            var events = Fixture.CreateDistinctCollection<int>( count: 3 );
            var @base = new HistoryEventPublisher<int>( capacity: 3 );
            foreach ( var e in events )
                @base.Publish( e );

            var sut = new ConcurrentHistoryEventPublisher<int, HistoryEventPublisher<int>>( @base );

            sut.History.Should().BeSequentiallyEqualTo( @base.History );
        }

        [Fact]
        public void ClearHistory_ShouldCallBaseEventPublisherClearHistory()
        {
            var events = Fixture.CreateDistinctCollection<int>( count: 3 );
            var @base = new HistoryEventPublisher<int>( capacity: 3 );
            foreach ( var e in events )
                @base.Publish( e );

            var sut = new ConcurrentHistoryEventPublisher<int, HistoryEventPublisher<int>>( @base );

            sut.ClearHistory();

            @base.History.Should().BeEmpty();
        }

        [Fact]
        public async Task ClearHistory_ShouldAcquireLock()
        {
            var @base = new HistoryEventPublisher<int>( capacity: 1 );
            var sut = new ConcurrentHistoryEventPublisher<int, HistoryEventPublisher<int>>( @base );
            var sync = sut.Sync;

            var action = Lambda.Of( () => sut.ClearHistory() );

            await action.Should().AcquireLockOn( sync );
        }

        [Fact]
        public void HistoryEnumerator_ShouldAcquireLock()
        {
            var @base = new HistoryEventPublisher<int>( capacity: 1 );
            var sut = new ConcurrentHistoryEventPublisher<int, HistoryEventPublisher<int>>( @base );
            var sync = sut.Sync;

            using var result = sut.History.GetEnumerator();
            var hasLock = Monitor.IsEntered( sync );

            hasLock.Should().BeTrue();
        }

        [Fact]
        public void HistoryEnumeratorDispose_ShouldReleaseLock()
        {
            var @base = new HistoryEventPublisher<int>( capacity: 1 );
            var sut = new ConcurrentHistoryEventPublisher<int, HistoryEventPublisher<int>>( @base );
            var sync = sut.Sync;

            var result = sut.History.GetEnumerator();
            result.Dispose();
            var hasLock = Monitor.IsEntered( sync );

            hasLock.Should().BeFalse();
        }

        [Fact]
        public void ToConcurrentExtension_ShouldCreateConcurrentWrapperForEventPublisher()
        {
            var @base = new HistoryEventPublisher<int>( capacity: 1 );
            var sut = @base.ToConcurrent();
            sut.Should().BeOfType<ConcurrentHistoryEventPublisher<int, HistoryEventPublisher<int>>>();
        }
    }
}
