using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.Core.Functional.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.TypeCast
{
    public abstract class GenericInvalidTypeCastExtensionsTests<TSource, TDestination> : TestsBase
        where TDestination : notnull
    {
        [Fact]
        public void ToMaybe_ShouldReturnWithoutValue_WhenIsInvalid()
        {
            var value = Fixture.Create<TSource>();

            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.ToMaybe();

            result.HasValue.Should().BeFalse();
        }
    }
}
