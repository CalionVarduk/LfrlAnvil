using System;
using Xunit;
using System.Collections.Generic;
using LfrlSoft.NET.Common.Extensions;
using LfrlSoft.NET.TestExtensions;
using AutoFixture;
using FluentAssertions;

namespace LfrlSoft.NET.Common.Tests.Extensions.Dictionary
{
    public abstract class DictionaryExtensionsTests<TKey, TValue> : TestsBase
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

            result.Should().Be( default( TValue? ) );
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

            result.Should().Be( providedValue );
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

            result.Should().Be( providedValue );
        }
    }
}
