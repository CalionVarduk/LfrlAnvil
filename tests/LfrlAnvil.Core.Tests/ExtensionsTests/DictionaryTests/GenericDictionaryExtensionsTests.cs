using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.DictionaryTests
{
    public abstract class GenericDictionaryExtensionsTests<TKey, TValue> : TestsBase
        where TKey : notnull
    {
        [Fact]
        public void GetOrAddDefault_ShouldReturnExistingValue()
        {
            var key = Fixture.Create<TKey>();
            var value = Fixture.Create<TValue>();
            var sut = new Dictionary<TKey, TValue?> { { key, value } };

            var result = sut.GetOrAddDefault( key );

            result.Should().Be( value );
        }

        [Fact]
        public void GetOrAddDefault_ShouldAddAndReturnDefaultValue_WhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();
            var sut = new Dictionary<TKey, TValue?>();

            var result = sut.GetOrAddDefault( key );

            using ( new AssertionScope() )
            {
                result.Should().Be( default( TValue? ) );
                sut[key].Should().Be( default( TValue? ) );
            }
        }

        [Fact]
        public void GetOrAdd_ShouldReturnExistingValue()
        {
            var key = Fixture.Create<TKey>();
            var (value, providedValue) = Fixture.CreateDistinctCollection<TValue>( 2 );
            var sut = new Dictionary<TKey, TValue> { { key, value } };

            var result = sut.GetOrAdd( key, () => providedValue );

            result.Should().Be( value );
        }

        [Fact]
        public void GetOrAdd_ShouldAddAndReturnProvidedValue_WhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();
            var providedValue = Fixture.Create<TValue>();
            var sut = new Dictionary<TKey, TValue>();

            var result = sut.GetOrAdd( key, () => providedValue );

            using ( new AssertionScope() )
            {
                result.Should().Be( providedValue );
                sut[key].Should().Be( providedValue );
            }
        }

        [Fact]
        public void GetOrAdd_WithLazy_ShouldReturnExistingValue()
        {
            var key = Fixture.Create<TKey>();
            var (value, providedValue) = Fixture.CreateDistinctCollection<TValue>( 2 );
            var sut = new Dictionary<TKey, TValue> { { key, value } };

            var result = sut.GetOrAdd( key, new Lazy<TValue>( () => providedValue ) );

            result.Should().Be( value );
        }

        [Fact]
        public void GetOrAdd_WithLazy_ShouldAddAndReturnProvidedValue_WhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();
            var providedValue = Fixture.Create<TValue>();
            var sut = new Dictionary<TKey, TValue>();

            var result = sut.GetOrAdd( key, new Lazy<TValue>( () => providedValue ) );

            using ( new AssertionScope() )
            {
                result.Should().Be( providedValue );
                sut[key].Should().Be( providedValue );
            }
        }

        [Fact]
        public void AddOrUpdate_ShouldAddValue_WhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();
            var value = Fixture.Create<TValue>();
            var sut = new Dictionary<TKey, TValue>();

            var result = sut.AddOrUpdate( key, value );

            using ( new AssertionScope() )
            {
                result.Should().Be( AddOrUpdateResult.Added );
                sut[key].Should().Be( value );
            }
        }

        [Fact]
        public void AddOrUpdate_ShouldUpdateValue_WhenKeyExists()
        {
            var key = Fixture.Create<TKey>();
            var (oldValue, newValue) = Fixture.CreateDistinctCollection<TValue>( 2 );
            var sut = new Dictionary<TKey, TValue> { { key, oldValue } };

            var result = sut.AddOrUpdate( key, newValue );

            using ( new AssertionScope() )
            {
                result.Should().Be( AddOrUpdateResult.Updated );
                sut[key].Should().Be( newValue );
            }
        }

        [Fact]
        public void TryUpdate_ShouldReturnFalse_WhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();
            var value = Fixture.Create<TValue>();
            var sut = new Dictionary<TKey, TValue>();

            var result = sut.TryUpdate( key, value );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 0 );
            }
        }

        [Fact]
        public void TryUpdate_ShouldReturnTrue_WhenKeyExists()
        {
            var key = Fixture.Create<TKey>();
            var (oldValue, newValue) = Fixture.CreateDistinctCollection<TValue>( 2 );
            var sut = new Dictionary<TKey, TValue> { { key, oldValue } };

            var result = sut.TryUpdate( key, newValue );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut[key].Should().Be( newValue );
            }
        }
    }
}
