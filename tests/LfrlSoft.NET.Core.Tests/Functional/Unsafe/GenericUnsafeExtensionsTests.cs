using System;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.Core.Functional.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Unsafe
{
    public abstract class GenericUnsafeExtensionsTests<T> : TestsBase
        where T : notnull
    {
        [Fact]
        public void ToMaybe_ShouldReturnWithValue_WhenHasNonNullValue()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = (Unsafe<T>) value;

            var result = sut.ToMaybe();

            using ( new AssertionScope() )
            {
                result.HasValue.Should().BeTrue();
                result.Value.Should().Be( value );
            }
        }

        [Fact]
        public void ToMaybe_ShouldReturnWithoutValue_WhenHasError()
        {
            var error = new Exception();

            var sut = (Unsafe<T>) error;

            var result = sut.ToMaybe();

            result.HasValue.Should().BeFalse();
        }

        [Fact]
        public void ToEither_ShouldReturnWithFirst_WhenHasValue()
        {
            var value = Fixture.Create<T>();

            var sut = (Unsafe<T>) value;

            var result = sut.ToEither();

            using ( new AssertionScope() )
            {
                result.HasFirst.Should().BeTrue();
                result.First.Should().Be( value );
            }
        }

        [Fact]
        public void ToEither_ShouldReturnWithSecond_WhenHasError()
        {
            var error = new Exception();

            var sut = (Unsafe<T>) error;

            var result = sut.ToEither();

            using ( new AssertionScope() )
            {
                result.HasSecond.Should().BeTrue();
                result.Second.Should().Be( error );
            }
        }
    }
}
