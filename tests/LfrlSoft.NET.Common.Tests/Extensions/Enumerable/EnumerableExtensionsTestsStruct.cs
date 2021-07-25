using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Extensions.Enumerable
{
    public abstract class EnumerableExtensionsTestsStruct<T> : EnumerableExtensionsTests<T>
        where T : struct
    {
        [Fact]
        public void WhereNotNull_ShouldFilterOutNullElements()
        {
            var expected = Fixture.CreateMany<T?>();
            var sut = expected.Append( null );

            var result = sut.WhereNotNull();

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void WhereNotNull_ShouldReturnFalseWhenSourceContainsNullElement()
        {
            var sut = Fixture.CreateMany<T?>();

            var result = sut.WhereNotNull();

            result.Should().BeEquivalentTo( sut );
        }

        [Fact]
        public void WhereNotNull_ShouldFilterOutNullElements_WithExplicitComparer_AndNullableType()
        {
            var expected = Fixture.CreateMany<T?>();
            var sut = expected.Append( null );

            var result = sut.WhereNotNull( EqualityComparer<T?>.Default );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void WhereNotNull_ShouldReturnFalseWhenSourceContainsNullElement_WithExplicitComparer_AndNullableType()
        {
            var sut = Fixture.CreateMany<T?>();

            var result = sut.WhereNotNull( EqualityComparer<T?>.Default );

            result.Should().BeEquivalentTo( sut );
        }

        [Fact]
        public void WhereNotNull_ShouldReturnSource_WithExplicitComparer()
        {
            var sut = Fixture.CreateMany<T>();

            var result = sut.WhereNotNull( EqualityComparer<T>.Default );

            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void ContainsNull_ShouldReturnTrueWhenSourceContainsNullElement()
        {
            var sut = Fixture.CreateMany<T?>().Append( null );

            var result = sut.ContainsNull();

            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsNull_ShouldReturnFalseWhenSourceContainsNullElement()
        {
            var sut = Fixture.CreateMany<T?>();

            var result = sut.ContainsNull();

            result.Should().BeFalse();
        }

        [Fact]
        public void ContainsNull_ShouldReturnFalse_WithExplicitComparer()
        {
            var sut = Fixture.CreateMany<T>();

            var result = sut.ContainsNull( EqualityComparer<T>.Default );

            result.Should().BeFalse();
        }

        [Fact]
        public void ContainsNull_ShouldReturnTrueWhenSourceContainsNullElement_WithExplicitComparer_AndNullableType()
        {
            var sut = Fixture.CreateMany<T?>().Append( null );

            var result = sut.ContainsNull( EqualityComparer<T?>.Default );

            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsNull_ShouldReturnFalseWhenSourceContainsNullElement_WithExplicitComparer_AndNullableType()
        {
            var sut = Fixture.CreateMany<T?>();

            var result = sut.ContainsNull( EqualityComparer<T?>.Default );

            result.Should().BeFalse();
        }
    }
}
