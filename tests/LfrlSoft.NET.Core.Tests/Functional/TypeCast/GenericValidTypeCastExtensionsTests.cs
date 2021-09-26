using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.Core.Functional.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.TypeCast
{
    public abstract class GenericValidTypeCastExtensionsTests<TSource, TDestination> : TestsBase
        where TSource : TDestination
        where TDestination : notnull
    {
        [Fact]
        public void ToMaybe_ShouldReturnWithValue_WhenIsValid()
        {
            var value = Fixture.CreateNotDefault<TSource>();

            var sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.ToMaybe();

            using ( new AssertionScope() )
            {
                result.HasValue.Should().BeTrue();
                result.Value.Should().Be( value );
            }
        }
    }
}
