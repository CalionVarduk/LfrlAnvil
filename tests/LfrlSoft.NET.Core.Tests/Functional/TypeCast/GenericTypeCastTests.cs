using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Functional.TypeCast
{
    public abstract class GenericTypeCastTests<TSource, TDestination> : TestsBase
    {
        [Fact]
        public void Empty_ShouldBeInvalid()
        {
            var sut = TypeCast<TSource, TDestination>.Empty;

            using ( new AssertionScope() )
            {
                sut.IsValid.Should().BeFalse();
                sut.IsInvalid.Should().BeTrue();
                sut.Source.Should().Be( default( TSource ) );
                sut.Result.Should().Be( default( TDestination ) );
            }
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<TSource>();
            var sut = (TypeCast<TSource, TDestination>)value;
            var expected = Core.Hash.Default.Add( value ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Fact]
        public void TypeCastConversionOperator_FromNil_ShouldReturnCorrectResult()
        {
            var result = (TypeCast<TSource, TDestination>)Core.Functional.Nil.Instance;

            using ( new AssertionScope() )
            {
                result.IsValid.Should().BeFalse();
                result.IsInvalid.Should().BeTrue();
                result.Source.Should().Be( default( TSource ) );
                result.Result.Should().Be( default( TDestination ) );
            }
        }

        [Fact]
        public void ITypeCastSource_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<TSource>();

            ITypeCast<TDestination> sut = (TypeCast<TSource, TDestination>)value;

            var result = sut.Source;

            result.Should().Be( value );
        }
    }
}
