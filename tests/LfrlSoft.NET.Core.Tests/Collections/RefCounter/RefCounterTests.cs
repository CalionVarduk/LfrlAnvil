using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Collections;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Collections.RefCounter
{
    public abstract class RefCounterTests<TKey> : TestsBase
        where TKey : notnull
    {
        [Fact]
        public void Ctor_ShouldCreateEmptyRefCounter()
        {
            var sut = new RefCounter<TKey>();

            sut.Count.Should().Be( 0 );
        }

        [Fact]
        public void Ctor_ShouldCreateWithExplicitComparer()
        {
            var comparer = EqualityComparer<TKey>.Default;

            var sut = new RefCounter<TKey>( comparer );

            sut.Comparer.Should().Be( comparer );
        }

        [Fact]
        public void Increment_ShouldAddNewReferenceWithValueEqualToOne()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            var result = sut.Increment( key );

            using ( new AssertionScope() )
            {
                result.Should().Be( 1 );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Increment_ShouldIncrementExistingReferenceByOne()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.Increment( key );

            var result = sut.Increment( key );

            using ( new AssertionScope() )
            {
                result.Should().Be( 2 );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Increment_ShouldThrowWhenReferenceCountIsTooLarge()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.IncrementBy( key, int.MaxValue );

            Action action = () => sut.Increment( key );

            action.Should().Throw<OverflowException>();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void IncrementBy_ShouldAddNewReferenceWithValueEqualToCount(int count)
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            var result = sut.IncrementBy( key, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( count );
                sut.Count.Should().Be( 1 );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void IncrementBy_ShouldIncrementExistingReferenceByCount(int count)
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.Increment( key );

            var result = sut.IncrementBy( key, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( 1 + count );
                sut.Count.Should().Be( 1 );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( -1 )]
        public void IncrementBy_ShouldThrowWhenCountIsLessThanOne(int count)
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            Action action = () => sut.IncrementBy( key, count );

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void IncrementBy_ShouldThrowWhenReferenceCountIsTooLarge()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.IncrementBy( key, int.MaxValue );

            Action action = () => sut.IncrementBy( key, 1 );

            action.Should().Throw<OverflowException>();
        }

        [Fact]
        public void Decrement_ShouldReturnMinusOneWhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            var result = sut.Decrement( key );

            result.Should().Be( -1 );
        }

        [Fact]
        public void Decrement_ShouldDecrementExistingReferenceByOne()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.IncrementBy( key, 2 );

            var result = sut.Decrement( key );

            using ( new AssertionScope() )
            {
                result.Should().Be( 1 );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Decrement_ShouldRemoveExistingReferenceByOneWhenRefCountIsEqualToOne()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.Increment( key );

            var result = sut.Decrement( key );

            using ( new AssertionScope() )
            {
                result.Should().Be( 0 );
                sut.Count.Should().Be( 0 );
            }
        }

        [Fact]
        public void DecrementBy_ShouldReturnMinusOneWhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            var result = sut.DecrementBy( key, 1 );

            result.Should().Be( -1 );
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void DecrementBy_ShouldDecrementExistingReferenceByCount(int count)
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.IncrementBy( key, 4 );

            var result = sut.DecrementBy( key, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( 4 - count );
                sut.Count.Should().Be( 1 );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void DecrementBy_ShouldRemoveExistingReferenceByCountWhenRefCountIsLessThanOrEqualToDecrementCount(int count)
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.IncrementBy( key, 1 );

            var result = sut.DecrementBy( key, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( 0 );
                sut.Count.Should().Be( 0 );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( -1 )]
        public void DecrementBy_ShouldThrowWhenCountIsLessThanOne(int count)
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            Action action = () => sut.DecrementBy( key, count );

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Remove_ShouldReturnTrueWhenKeyExistsAndRemoveIt()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.Increment( key );

            var result = sut.Remove( key );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalseWhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            var result = sut.Remove( key );

            result.Should().BeFalse();
        }

        [Fact]
        public void Clear_ShouldRemoveAllEntries()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );

            var sut = new RefCounter<TKey>();

            foreach ( var key in keys )
                sut.Increment( key );

            sut.Clear();

            sut.Count.Should().Be( 0 );
        }

        [Fact]
        public void ContainsKey_ShouldReturnTrueWhenKeyExists()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.Increment( key );

            var result = sut.ContainsKey( key );

            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsKey_ShouldReturnFalseWhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            var result = sut.ContainsKey( key );

            result.Should().BeFalse();
        }

        [Fact]
        public void TryGetValue_ShouldReturnCorrectResultWhenKeyExists()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.IncrementBy( key, 5 );

            var result = sut.TryGetValue( key, out var count );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                count.Should().Be( 5 );
            }
        }

        [Fact]
        public void TryGetValue_ShouldReturnFalseWhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            var result = sut.TryGetValue( key, out var count );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                count.Should().Be( default );
            }
        }

        [Fact]
        public void DecrementBy_ShouldThrowWhenReferenceCountIsTooLarge()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.IncrementBy( key, int.MaxValue );

            Action action = () => sut.IncrementBy( key, 1 );

            action.Should().Throw<OverflowException>();
        }

        [Fact]
        public void Keys_ShouldReturnCorrectResult()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );

            var sut = new RefCounter<TKey>();

            for ( var i = 0; i < keys.Count; ++i )
                sut.IncrementBy( keys[i], i + 1 );

            sut.Keys.Should().BeEquivalentTo( keys );
        }

        [Fact]
        public void Values_ShouldReturnCorrectResult()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var expected = new[] { 1, 2, 3 }.AsEnumerable();

            var sut = new RefCounter<TKey>();

            for ( var i = 0; i < keys.Count; ++i )
                sut.IncrementBy( keys[i], i + 1 );

            sut.Values.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void IndexerGet_ShouldReturnCorrectResultWhenKeyExists()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();
            sut.IncrementBy( key, 5 );

            var result = sut[key];

            result.Should().Be( 5 );
        }

        [Fact]
        public void IndexerGet_ShouldThrowWhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();

            var sut = new RefCounter<TKey>();

            Action action = () =>
            {
                var _ = sut[key];
            };

            action.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var expected = new[]
                {
                    KeyValuePair.Create( keys[0], 1 ),
                    KeyValuePair.Create( keys[1], 2 ),
                    KeyValuePair.Create( keys[2], 3 )
                }
                .AsEnumerable();

            var sut = new RefCounter<TKey>();

            for ( var i = 0; i < keys.Count; ++i )
                sut.IncrementBy( keys[i], i + 1 );

            sut.Should().BeEquivalentTo( expected );
        }
    }
}
