using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.TypeCast
{
    public abstract class GenericPartialTypeCastTests<TSource> : TestsBase
    {
        [Fact]
        public void Ctor_ShouldCreateWithCorrectValue()
        {
            var value = Fixture.Create<TSource>();
            var sut = new PartialTypeCast<TSource>( value );
            sut.Value.Should().Be( value );
        }
    }
}
