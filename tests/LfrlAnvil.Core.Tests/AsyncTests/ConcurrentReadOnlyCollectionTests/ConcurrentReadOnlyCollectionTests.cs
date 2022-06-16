using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Async;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Tests.AsyncTests.ConcurrentReadOnlyCollectionTests
{
    public class ConcurrentReadOnlyCollectionTests : TestsBase
    {
        [Fact]
        public void Ctor_ShouldCreateWithoutAcquiringAnyLocks()
        {
            var sync = new object();
            var _ = new ConcurrentReadOnlyCollection<int>( Array.Empty<int>(), sync );

            var hasLock = Monitor.IsEntered( sync );

            hasLock.Should().BeFalse();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 3 )]
        [InlineData( 10 )]
        public void Count_ShouldReturnResultEqualToUnderlyingCollectionsCount(int count)
        {
            var collection = Fixture.CreateMany<int>( count ).ToList();
            var sync = new object();
            var sut = new ConcurrentReadOnlyCollection<int>( collection, sync );

            var result = sut.Count;

            result.Should().Be( count );
        }

        [Fact]
        public void Count_ShouldAcquireLock()
        {
            var sync = new object();
            var hasLock = false;

            var collection = Substitute.For<IReadOnlyCollection<int>>();
            collection.Count.Returns(
                _ =>
                {
                    hasLock = Monitor.IsEntered( sync );
                    return Fixture.Create<int>();
                } );

            var sut = new ConcurrentReadOnlyCollection<int>( collection, sync );

            var _ = sut.Count;

            hasLock.Should().BeTrue();
        }

        [Fact]
        public void GetEnumerator_ShouldReturnEnumeratorEquivalentToUnderlyingCollectionEnumerator()
        {
            var collection = Fixture.CreateMany<int>().ToList();
            var sync = new object();
            var sut = new ConcurrentReadOnlyCollection<int>( collection, sync );

            sut.Should().BeSequentiallyEqualTo( collection );
        }

        [Fact]
        public void GetEnumerator_ShouldAcquireLock()
        {
            var sync = new object();
            var sut = new ConcurrentReadOnlyCollection<int>( Array.Empty<int>(), sync );

            using var _ = sut.GetEnumerator();
            var hasLock = Monitor.IsEntered( sync );

            hasLock.Should().BeTrue();
        }

        [Fact]
        public void GetEnumerator_ShouldReleaseLock_WhenEnumeratorInstanceIsDisposed()
        {
            var sync = new object();
            var sut = new ConcurrentReadOnlyCollection<int>( Array.Empty<int>(), sync );

            var enumerator = sut.GetEnumerator();
            enumerator.Dispose();

            var hasLock = Monitor.IsEntered( sync );

            hasLock.Should().BeFalse();
        }
    }
}
