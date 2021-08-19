using System;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.Core.Functional.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.Either
{
    public abstract class GenericEitherExtensionsTests<T1, T2> : TestsBase
        where T1 : notnull
    {
        [Fact]
        public void ToMaybe_ShouldReturnWithValue_WhenHasNonNullFirst()
        {
            var value = Fixture.CreateNotDefault<T1>();

            var sut = (Either<T1, T2>) value;

            var result = sut.ToMaybe();

            using ( new AssertionScope() )
            {
                result.HasValue.Should().BeTrue();
                result.Value.Should().Be( value );
            }
        }

        [Fact]
        public void ToMaybe_ShouldReturnWithoutValue_WhenHasSecond()
        {
            var value = Fixture.Create<T2>();

            var sut = (Either<T1, T2>) value;

            var result = sut.ToMaybe();

            result.HasValue.Should().BeFalse();
        }

        [Fact]
        public void ToUnsafe_ShouldReturnOk_WhenHasFirst()
        {
            var value = Fixture.Create<T1>();

            var sut = (Either<T1, Exception>) value;

            var result = sut.ToUnsafe();

            using ( new AssertionScope() )
            {
                result.IsOk.Should().BeTrue();
                result.Value.Should().Be( value );
            }
        }

        [Fact]
        public void ToUnsafe_ShouldReturnWithError_WhenHasSecond()
        {
            var error = new Exception();

            var sut = (Either<T1, Exception>) error;

            var result = sut.ToUnsafe();

            using ( new AssertionScope() )
            {
                result.HasError.Should().BeTrue();
                result.Error.Should().Be( error );
            }
        }
    }
}
